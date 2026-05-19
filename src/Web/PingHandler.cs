namespace Rhubarb.Web;

using System.Diagnostics;
using Rebus.Bus;
using Rebus.Handlers;
using Rheum.Shared;

public sealed class PingHandler(IBus bus, ILogger<PingHandler> logger, TimeProvider timeProvider) : IHandleMessages<Ping>
{
    public async Task Handle(Ping message)
    {
        var activity = Activity.Current;
        activity?.SetTag("messaging.machine", Environment.MachineName);
        activity?.SetTag("ping.message", message.Message);

        logger.LogInformation("Received ping: {Message} on {Machine}", message.Message, Environment.MachineName);

        var dateTime = timeProvider.GetUtcNow().DateTime;

        var pong = new Pong
        {
            Message = $"{message.Message} => {dateTime.Ticks}",
            PingSentAtUtcTicks = message.SentAtUtcTicks,
        };
        logger.LogInformation("Sending pong: {Message} on {Machine}", pong.Message, Environment.MachineName);

        await bus.Reply(pong);
    }
}
