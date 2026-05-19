namespace Rhubarb.Web;

using Rebus.Bus;
using Rebus.Handlers;
using Rheum.Shared;

public sealed class PingHandler(IBus bus, ILogger<PingHandler> logger, TimeProvider timeProvider) : IHandleMessages<Ping>
{
    public async Task Handle(Ping message)
    {
        logger.LogInformation("Received ping: {Message}", message.Message);

        var dateTime = timeProvider.GetUtcNow().DateTime;

        var pong = new Pong { Message = $"{message.Message} => {dateTime.Ticks}" };
        logger.LogInformation("Sending pong: {Message}", pong.Message);

        await bus.Reply(pong);
    }
}
