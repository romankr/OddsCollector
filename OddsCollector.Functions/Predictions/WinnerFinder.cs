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

        var best = calculator.GetScores(odds).MaxBy(p => p.Score)!;

        // Nothing above zero means no bookmaker quoted anything usable, and every outcome
        // scored the same nothing. MaxBy would return the first of the three, which reads
        // downstream as a confident prediction for it. A real market always leaves one
        // outcome above zero: the implied probabilities sum past one, while the adjustments
        // between them only take off 0.128.
        if (best.Score <= 0)
        {
            throw new ArgumentException($"{nameof(odds)} has no usable quote", nameof(odds));
        }

        return best.Outcome;
    }
}
