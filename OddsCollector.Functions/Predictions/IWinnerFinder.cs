using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal interface IWinnerFinder
{
    string GetWinner(ICollection<Odd> odds);
}
