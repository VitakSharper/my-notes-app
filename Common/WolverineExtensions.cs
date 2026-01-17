using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Common;

public static class WolverineExtensions
{
    public static async Task UseWolverineWithRabbitMqAsync(this IHostApplicationBuilder builder, Action<WolverineOptions> configureMessaging)
    {
        var retryPolicy = Policy
            .Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"[RabbitMQ] Retry {retryCount}/10 - {exception.GetType().Name}. Waiting {timeSpan.TotalSeconds:F0}s before next retry...");
                });

        await retryPolicy.ExecuteAsync(async () =>
        {
            var endpoint = builder.Configuration.GetConnectionString("messaging")
                           ?? throw new InvalidOperationException("RabbitMQ connection string not found.");

            Console.WriteLine($"[RabbitMQ] Attempting to connect to: {endpoint}");

            var factory = new ConnectionFactory()
            {
                Uri = new Uri(endpoint)
            };

            await using var connection = await factory.CreateConnectionAsync();
            Console.WriteLine("[RabbitMQ] Connection successful!");
        });

        builder.Services.AddOpenTelemetry().WithTracing(traceProviderBuilder =>
        {
            traceProviderBuilder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(builder.Environment.ApplicationName))
                .AddSource("Wolverine");
        });

        builder.UseWolverine(options =>
        {
            options.UseRabbitMqUsingNamedConnection("messaging")
                .AutoProvision()
                .DeclareExchange("questions");

            configureMessaging(options);
        });
    }
}