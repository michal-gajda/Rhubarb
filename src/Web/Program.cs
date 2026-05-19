namespace Rhubarb.Web;

using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Kafka;
using Rebus.OpenTelemetry.Configuration;
using Rheum.Shared;

public sealed class Program
{
    private Program()
    {
    }

    public static async Task<int> Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddHealthChecks()
            .AddKafka(options =>
            {
                options.BootstrapServers = builder.Configuration.GetConnectionString("Kafka");
            });

        string serviceName = ServiceConstants.ServiceName;
        const string serviceNamespace = "Rhubarb";
        string serviceVersion = ServiceConstants.ServiceVersion;
        string serviceInstanceId = Environment.GetEnvironmentVariable("HOSTNAME") ?? Environment.MachineName;

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName, serviceNamespace, serviceVersion, autoGenerateServiceInstanceId: false, serviceInstanceId: serviceInstanceId);

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(resourceBuilder);
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;
            options.AddOtlpExporter();
        });

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .SetResourceBuilder(resourceBuilder)
                .SetSampler(new AlwaysOnSampler())
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.Filter = context => !context.Request.Path.StartsWithSegments("/health");
                })
                .AddHttpClientInstrumentation(options => options.RecordException = true)
                .AddRebusInstrumentation()
                .AddRhubarbInstrumentation()
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddProcessInstrumentation()
                .AddRebusInstrumentation()
                .AddRhubarbInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter());

        var configuration = builder.Configuration;
        var connectionString = configuration.GetConnectionString("Kafka");
        var queueName = "ping-service-topic";

        builder.Services.AddTransient<IHandleMessages<Ping>, PingHandler>();
        builder.Services.AddRebus(configure => configure
            .Transport(transport => transport.UseKafka(connectionString, queueName))
            .Options(options => options.EnableDiagnosticSources())
        );
        builder.Services.AutoRegisterHandlersFromAssemblyOf<PingHandler>();

        builder.Services.AddSingleton(TimeProvider.System);

        var app = builder.Build();

        app.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        await app.RunAsync();

        return Environment.ExitCode;
    }
}
