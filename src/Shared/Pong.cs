namespace Rheum.Shared;

public sealed record class Pong
{
    public required string Message { get; init; }
    public required long PingSentAtUtcTicks { get; init; }
}
