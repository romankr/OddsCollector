using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal sealed class OutcomePredictor(IScoreCalculator calculator) : IOutcomePredictor
{
    public string GetOutcome(ICollection<Odd> odds)
    {
        if (odds.Count == 0)
        {
            throw new ArgumentException($"{nameof(odds)} cannot be empty", nameof(odds));
        }

        var best = calculator.GetScores(odds).MaxBy(p => p.Score)!;

        if (best.Score <= 0)
        {
            throw new ArgumentException($"{nameof(odds)} has no usable quote", nameof(odds));
        }

        return best.Outcome;
    }
}
