using Microsoft.Extensions.Hosting;
#pragma warning disable ASPIRECERTIFICATES001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

Environment.SetEnvironmentVariable("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true");

var builder = DistributedApplication.CreateBuilder(args);

var compose = builder.AddDockerComposeEnvironment("production")
    .WithDashboard(dashboard => dashboard.WithHostPort(8080));

var keycloak = builder.AddKeycloak("keycloak", 6001)
    .WithDataVolume("keycloak-data")
    .WithoutHttpsCertificate()
    .WithRealmImport("../Overflow.AppHost/infra/realms")
    .WithEnvironment("KC_HTTP_ENABLED", "true")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    .WithEnvironment("KC_PROXY_HEADERS", "xforwarded")
    // Pins the frontend URL, so a token minted through the internal keycloak:8080 endpoint still
    // carries iss = id.overflow.local. Without it Keycloak derives the issuer from the request
    // host, and Auth.js rejects the id_token it gets back from the server-to-server exchange.
    // Backchannel dynamic is what keeps that internal call legal while the hostname is a full URL.
    .WithEnvironment("KC_HOSTNAME", "https://id.overflow.local")
    .WithEnvironment("KC_HOSTNAME_BACKCHANNEL_DYNAMIC", "true")
    .WithEnvironment("VIRTUAL_HOST", "id.overflow.local")
    .WithEnvironment("VIRTUAL_PORT", "8080");
//.WithEndpoint(port: 6001, targetPort: 8080, isExternal: true);

var sql = builder.AddSqlServer("sql", port: 1433)
    .WithImageTag("2025-latest")
    .WithDataVolume("sql-data")
    .WithLifetime(ContainerLifetime.Persistent);

var typeSenseApiKey = builder.AddParameter("typesense-api-key", secret: true);

var typeSense = builder.AddContainer("typesense", "typesense/typesense", "29.0")
    .WithVolume(name: "typesense-data", target: "/data")
    .WithEnvironment("TYPESENSE_API_KEY", typeSenseApiKey)
    .WithEnvironment("TYPESENSE_DATA_DIR", "/data")
    .WithEnvironment("TYPESENSE_ENABLE_CORS", "true")
    .WithHttpEndpoint(port: 8108, targetPort: 8108, name: "typesense");

var typeSenseContainer = typeSense.GetEndpoint("typesense");

// S3-compatible object storage for the images pasted into the rich text editor. The course uses
// Cloudinary; MinIO keeps the same shape (upload returns a URL plus a key we can delete by)
// without an external account. Port 9000 is the S3 API, 9001 the web console.
var minioUser = builder.AddParameter("minio-user");
var minioPassword = builder.AddParameter("minio-password", secret: true);

var minio = builder.AddContainer("minio", "minio/minio", "RELEASE.2025-04-22T22-12-26Z")
    .WithArgs("server", "/data", "--console-address", ":9001")
    .WithEnvironment("MINIO_ROOT_USER", minioUser)
    .WithEnvironment("MINIO_ROOT_PASSWORD", minioPassword)
    .WithVolume("minio-data", "/data")
    // isExternal is what turns these into published host ports in the generated compose file:
    // without it Aspire only writes `expose`, and the browser could not load an image.
    .WithEndpoint(port: 9000, targetPort: 9000, scheme: "http", name: "minio", isExternal: true)
    .WithEndpoint(port: 9001, targetPort: 9001, scheme: "http", name: "minio-console",
        isExternal: true)
    // The images are loaded by the browser, so once the client app is served over HTTPS they have
    // to be too, or they count as mixed content and get blocked. Behind the proxy for that reason
    // (the course keeps Cloudinary here, which is HTTPS already).
    .WithEnvironment("VIRTUAL_HOST", "minio.overflow.local")
    .WithEnvironment("VIRTUAL_PORT", "9000");

var minioEndpoint = minio.GetEndpoint("minio");

var questionDb = sql.AddDatabase("questionDb");

var rabbitMq = builder.AddRabbitMQ("messaging")
    .WithDataVolume("rabbitmq-data")
    .WithManagementPlugin(port: 15672);

var questionService = builder.AddProject<Projects.QuestionService>("question-svc")
    .WithReference(keycloak)
    .WithReference(questionDb)
    .WithReference(rabbitMq)
    .WaitFor(keycloak)
    .WaitFor(questionDb)
    .WaitFor(rabbitMq);

var searchService = builder.AddProject<Projects.SearchService>("search-svc")
    .WithEnvironment("typesense-api-key", typeSenseApiKey)
    .WithReference(typeSenseContainer)
    .WithReference(rabbitMq)
    .WaitFor(typeSense)
    .WaitFor(rabbitMq);



var yarp = builder.AddYarp("gateway")
    .WithImageRegistry("mcr.microsoft.com")
    .WithImage("dotnet/nightly/yarp")
    .WithImageTag("latest")
    .WithConfiguration(yarpBuilder =>
    {
        yarpBuilder.AddRoute("/questions/{**catch-all}", questionService);
        yarpBuilder.AddRoute("/tags/{**catch-all}", questionService);
        yarpBuilder.AddRoute("/test/{**catch-all}", questionService);
        yarpBuilder.AddRoute("/search/{**catch-all}", searchService);
    })
    .WithoutHttpsCertificate()
    .WithEnvironment("ASPNETCORE_URLS", "http://*:8001")
    .WithEndpoint(port: 8001, scheme: "http", targetPort: 8001, name: "gateway", isExternal: true)
    .WithEnvironment("VIRTUAL_HOST", "api.overflow.local")
    .WithEnvironment("VIRTUAL_PORT", "8001");

// Next.js client app. AddJavaScriptApp replaces AddNpmApp (Aspire.Hosting.NodeJs) since Aspire 13.
// The run script defaults to "dev"; PORT is injected so next dev binds the endpoint Aspire assigns.
var webApp = builder.AddJavaScriptApp("webapp", "../webapp")
    .WithReference(keycloak)
    .WithReference(yarp)
    // Server-side fetches resolve the gateway through Aspire instead of a hardcoded URL:
    // http://localhost:8001 in development, http://gateway:8001 under Docker Compose.
    .WithEnvironment("GATEWAY_URL", yarp.GetEndpoint("gateway"))
    // Image storage: the server side talks to MinIO through this endpoint, and the browser loads
    // the images from the public one - the same URL while the client runs on the host.
    .WithEnvironment("MINIO_ENDPOINT", minioEndpoint)
    .WithEnvironment("MINIO_ACCESS_KEY", minioUser)
    .WithEnvironment("MINIO_SECRET_KEY", minioPassword)
    .WithEnvironment("MINIO_BUCKET", "overflow")
    // The URL the browser uses. The client bundle inlines it at build time from .env.production,
    // but storage.ts reads it again server-side to build the URL it stores in the question HTML -
    // so the value injected here has to match, or uploads would come back over plain HTTP and the
    // HTTPS page would refuse to display them.
    .WithEnvironment("NEXT_PUBLIC_IMAGE_BASE_URL", builder.ExecutionContext.IsPublishMode
        ? "https://minio.overflow.local/overflow"
        : "http://localhost:9000/overflow")
    // nginx-proxy fronts the client app like it fronts the gateway and Keycloak. VIRTUAL_PORT is
    // the port inside the container, which the Dockerfile fixes at 3000.
    .WithEnvironment("VIRTUAL_HOST", "app.overflow.local")
    .WithEnvironment("VIRTUAL_PORT", "3000");

if (builder.ExecutionContext.IsPublishMode)
{
    // Compose only: the image built from webapp/Dockerfile runs the standalone server, and Aspire
    // passes PORT so it binds the target port. Published on 4000 rather than 3000 so a `next dev`
    // on the host keeps its usual port free.
    webApp.WithEndpoint(port: 4000, targetPort: 3000, scheme: "http", name: "http", env: "PORT",
            isExternal: true)
        .PublishAsDockerFile();
}
else
{
    webApp.WithHttpEndpoint(port: 3000, env: "PORT");
}

// nginx-proxy watches the Docker socket and auto-generates a reverse proxy config for every
// container exposing VIRTUAL_HOST. Docker Compose only - Aspire orchestrates itself in development.
if (!builder.Environment.IsDevelopment())
{
    builder.AddContainer("nginx-proxy", "nginxproxy/nginx-proxy", "1.8")
        .WithEndpoint(port: 80, targetPort: 80, scheme: "http", name: "nginx", isExternal: true)
        // Two endpoints cannot share a name, hence nginx-ssl. SSL terminates here; everything
        // behind the proxy stays on plain HTTP inside the compose network.
        .WithEndpoint(port: 443, targetPort: 443, scheme: "https", name: "nginx-ssl",
            isExternal: true)
        .WithBindMount("/var/run/docker.sock", "/tmp/docker.sock", isReadOnly: true)
        // mkcert-issued certificate. nginx-proxy drops the leftmost label when looking for a cert,
        // so overflow.local.crt serves app., api., id. and minio.overflow.local alike.
        .WithBindMount("../Overflow.AppHost/infra/dev-certs", "/etc/nginx/certs", isReadOnly: true)
        // Default would be to redirect every http request to https. Plain http has to keep working
        // for the development client: `next dev` runs on the host, and Node's fetch ships its own
        // CA bundle - it ignores the Windows store where the mkcert root lives, so it would refuse
        // the certificate that the browser accepts.
        .WithEnvironment("HTTPS_METHOD", "noredirect");
}

builder.Build().Run();