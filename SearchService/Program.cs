using Common;
using Overflow.ServiceDefaults;
using SearchService.Data;
using SearchService.Endpoints;
using Typesense;
using Typesense.Setup;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.AddServiceDefaults();

await builder.UseWolverineWithRabbitMqAsync(options =>
{
    options.ListenToRabbitQueue("questions.search", cfg =>
    {
        cfg.BindExchange("questions");
    });

    options.ApplicationAssembly = typeof(Program).Assembly;
});

var typeSenseUri = builder.Configuration["services:typesense:typesense:0"] ??
                   throw new InvalidOperationException("Typesense URI is not configured.");

var typeSenseApiKey = builder.Configuration["typesense-api-key"] ??
                      throw new InvalidOperationException("Typesense key not found.");

var uri = new Uri(typeSenseUri);
builder.Services.AddTypesenseClient(config =>
{
    config.ApiKey = typeSenseApiKey;
    config.Nodes = new List<Node>
    {
        new(uri.Host, uri.Port.ToString(), uri.Scheme)
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

app.MapSearchEndpoints();

using var scope = app.Services.CreateScope();
var searchInitializer = scope.ServiceProvider.GetRequiredService<ITypesenseClient>();
await SearchInitializer.EnsureIndexExists(searchInitializer);

app.Run();