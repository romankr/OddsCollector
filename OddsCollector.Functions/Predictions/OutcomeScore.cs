using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal sealed record OutcomeScore
{
    public required double Score { get; init; }

    public required OutcomeType Outcome { get; init => field = Guard.Defined(value, nameof(Outcome)); }
}
