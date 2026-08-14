using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal interface IOutcomeFinder
{
    string GetOutcome(ICollection<Odd> odds);
}
