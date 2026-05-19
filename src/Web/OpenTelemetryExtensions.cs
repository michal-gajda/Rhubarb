namespace Rhubarb.Web;

using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

public static class OpenTelemetryExtensions
{
    public static MeterProviderBuilder AddRhubarbInstrumentation(this MeterProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddMeter(DomainMetrics.METER_NAME);
    }

    public static TracerProviderBuilder AddRhubarbInstrumentation(this TracerProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddSource("Rhubarb.Service");
    }
}
