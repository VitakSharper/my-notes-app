Environment.SetEnvironmentVariable("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true");

var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder.AddKeycloak("keycloak", 6001)
    .WithDataVolume("keycloak-data");

var sql = builder.AddSqlServer("sql", port: 1433)
    .WithImageTag("2025-latest")
    .WithDataVolume("sql-data")
    .WithLifetime(ContainerLifetime.Persistent);

var typeSenseApiKey = builder.AddParameter("typesense-api-key", secret: true);

var typeSense = builder.AddContainer("typesense", "typesense/typesense", "29.0")
    .WithArgs("--data-dir", "/data", "--api-key", typeSenseApiKey, "--enable-cors", "true")
    .WithVolume(name: "typesense-data", target: "/data")
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
    .WithEnvironment("ASPNETCORE_URLS", "http://*:8001")
    .WithEndpoint(port: 8001, scheme: "http", targetPort: 8001, name: "gateway", isExternal: true);

builder.Build().Run();