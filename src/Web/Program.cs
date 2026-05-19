namespace Rhubarb.Web;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Rebus.Config;
using Rebus.Kafka;

public sealed class Program
{
    private Program()
    {
    }

    public static async Task<int> Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHealthChecks();

        string serviceName = ServiceConstants.ServiceName;
        const string serviceNamespace = "Rheum";
        string serviceVersion = ServiceConstants.ServiceVersion;
        const string serviceInstanceId = "instance-1";

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
                .AddRhubarbInstrumentation()
                .AddRhubarbInstrumentation()
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddProcessInstrumentation()
                .AddRhubarbInstrumentation()
                .AddRhubarbInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter());

        var configuration = builder.Configuration;
        var connectionString = configuration.GetConnectionString("Kafka");
        var queueName = "rheum-queue";

        builder.Services.AddRebus(configure => configure.Transport(transport => transport.UseKafka(connectionString, queueName)));

        builder.Services.AddSingleton(TimeProvider.System);

        var app = builder.Build();

        app.UseHealthChecks("/healthz");

        await app.RunAsync();

        return Environment.ExitCode;
    }
}
