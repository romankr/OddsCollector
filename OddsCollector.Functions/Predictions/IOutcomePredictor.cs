using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal interface IOutcomePredictor
{
    /// <summary>Predicts the most likely outcome from the bookmakers' odds.</summary>
    /// <returns><see langword="false" /> when no odds are usable, so nothing can be predicted.</returns>
    bool TryGetOutcome(IReadOnlyCollection<Odd> odds, out OutcomeType outcome);
}
