using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal sealed record OutcomeScore
{
    public required double Score { get; init; }

    public required string Outcome { get; init => field = Guard.NotBlank(value, nameof(Outcome)); }
}
