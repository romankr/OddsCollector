using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal interface IOutcomePredictor
{
    bool TryGetOutcome(IReadOnlyCollection<Odd> odds, out OutcomeType outcome);
}
