using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Overflow.ServiceDefaults;
using Polly;
using QuestionService.Data;
using QuestionService.Data.Extensions;
using QuestionService.Data.Repositories;
using QuestionService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using Wolverine;
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

// Register repositories
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();

// Register services
builder.Services.AddMemoryCache();
builder.Services.AddScoped<TagService>();

builder.Services.AddOpenTelemetry().WithTracing(traceProviderBuilder =>
{
    traceProviderBuilder.SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService(builder.Environment.ApplicationName))
        .AddSource("Wolverine");
});

var retryPolicy = Policy
    .Handle<BrokerUnreachableException>()
    .Or<SocketException>()
    .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        (exception, timeSpan, retryCount) =>
        {
            Console.WriteLine($"Retry {retryCount} encountered {exception.GetType().Name}. Waiting {timeSpan} before next retry.");
        });

await retryPolicy.ExecuteAsync(async () =>
{
    var endpoint = builder.Configuration.GetConnectionString("messaging")
                   ?? throw new InvalidOperationException("RabbitMQ connection string not found.");

    var factory = new ConnectionFactory()
    {
        Uri = new Uri(endpoint)
    };

    await using var connection = await factory.CreateConnectionAsync();
});

builder.Host.UseWolverine(options =>
{
    options.UseRabbitMqUsingNamedConnection("messaging").AutoProvision();
    options.PublishAllMessages().ToRabbitExchange("questions");
});

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
