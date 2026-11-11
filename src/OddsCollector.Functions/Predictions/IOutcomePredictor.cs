using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal interface IOutcomePredictor
{
    OutcomeResult GetOutcome(IReadOnlyCollection<Odd> odds);
}
