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
    .WithEnvironment("KC_PROXY_HEADERS","xforwarded")
    .WithEnvironment("VIRTUAL_PORT", "8080");
    //.WithEndpoint(port: 6001, targetPort: 8080, isExternal: true);

var sql = builder.AddSqlServer("sql", port: 1433)
    .WithImageTag("2025-latest")
    .WithDataVolume("sql-data")
    .WithLifetime(ContainerLifetime.Persistent);

var typeSenseApiKey = builder.AddParameter("typesense-api-key", secret:true);

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
        yarpBuilder.AddRoute("/search/{**catch-all}", searchService);
    })
    .WithoutHttpsCertificate()
    .WithEnvironment("ASPNETCORE_URLS", "http://*:8001")
    .WithEndpoint(port: 8001, scheme: "http", targetPort: 8001, name: "gateway", isExternal: true);

builder.Build().Run();