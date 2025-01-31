using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal sealed class WinnerFinder(IScoreCalculator calculator) : IWinnerFinder
{
    public string GetWinner(ICollection<Odd> odds)
    {
        if (odds.Count == 0)
        {
            throw new ArgumentException($"{nameof(odds)} cannot be empty", nameof(odds));
        }

        return calculator.GetScores(odds).MaxBy(p => p.Score)!.Outcome;
    }
}
