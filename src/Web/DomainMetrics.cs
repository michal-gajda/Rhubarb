namespace Rhubarb.Web;

using System.Diagnostics.Metrics;

public static class DomainMetrics
{
    public const string METER_NAME = "Rhubarb.Service";
    private static readonly Meter Meter = new(METER_NAME, "1.0.0");

    public static readonly Histogram<double> ApplicationCommandDuration = Meter.CreateHistogram<double>(name: "rhubarb.application.command.duration", unit: "s", description: "The processing time for business commands in the application layer.");
}
