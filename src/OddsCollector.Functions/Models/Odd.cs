namespace OddsCollector.Functions.Models;

internal sealed record Odd
{
    public required double Away { get; init; }

    public required string Bookmaker { get; init => field = Guard.NotBlank(value, nameof(Bookmaker)); }

    public required double Draw { get; init; }

    public required double Home { get; init; }
}
