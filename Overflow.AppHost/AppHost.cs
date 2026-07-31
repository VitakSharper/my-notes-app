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
    .WithHttpEndpoint(port: 3000, env: "PORT");

// nginx-proxy watches the Docker socket and auto-generates a reverse proxy config for every
// container exposing VIRTUAL_HOST. Docker Compose only - Aspire orchestrates itself in development.
if (!builder.Environment.IsDevelopment())
{
    builder.AddContainer("nginx-proxy", "nginxproxy/nginx-proxy", "1.8")
        .WithEndpoint(port: 80, targetPort: 80, scheme: "http", name: "nginx", isExternal: true)
        .WithBindMount("/var/run/docker.sock", "/tmp/docker.sock", isReadOnly: true);
}

builder.Build().Run();