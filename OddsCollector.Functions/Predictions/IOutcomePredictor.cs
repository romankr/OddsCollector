using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal interface IOutcomePredictor
{
    string GetOutcome(ICollection<Odd> odds);
}
