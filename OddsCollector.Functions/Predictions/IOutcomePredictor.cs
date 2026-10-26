using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal interface IOutcomePredictor
{
    OutcomeType GetOutcome(IReadOnlyCollection<Odd> odds);
}
