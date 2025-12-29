using Common;
using Overflow.ServiceDefaults;
using QuestionService.Data;
using QuestionService.Data.Extensions;
using QuestionService.Extensions;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddServiceDefaults();

builder.Services.AddAuthentication()
    .AddKeycloakJwtBearer(serviceName: "keycloak", realm: "overflow", options =>
    {
        options.RequireHttpsMetadata = false;
        options.Audience = "overflow";
    });

builder.AddSqlServerDbContext<QuestionDbContext>("questionDb");

await builder.UseWolverineWithRabbitMqAsync(options =>
{
    options.PublishAllMessages().ToRabbitExchange("questions");
    options.ApplicationAssembly= typeof(Program).Assembly;
});

builder.Services.AddQuestionServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.MapDefaultEndpoints();

await app.MigrateDatabase();

app.Run();
