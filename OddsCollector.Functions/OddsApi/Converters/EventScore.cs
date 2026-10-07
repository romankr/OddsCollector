using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed record EventScore
{
    public required string Name { get; init => field = Guard.NotBlank(value, nameof(Name)); }

    public required int Score { get; init; }
}
