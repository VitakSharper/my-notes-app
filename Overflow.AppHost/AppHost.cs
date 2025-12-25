Environment.SetEnvironmentVariable("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true");

var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder.AddKeycloak("keycloak", 6001)
    .WithDataVolume("keycloak-data");

var sql = builder.AddSqlServer("sql")
    .WithImageTag("2025-latest")
    .WithLifetime(ContainerLifetime.Persistent);

var questionDb = sql.AddDatabase("questionDb");


var questionService = builder.AddProject<Projects.QuestionService>("question-svc")
    .WithReference(keycloak)
    .WithReference(questionDb)
    .WaitFor(keycloak)
    .WaitFor(questionDb);

builder.Build().Run();