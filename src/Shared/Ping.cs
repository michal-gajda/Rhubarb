namespace Rheum.Shared;

public sealed record class Ping
{
    public required string Message { get; init; }
}
