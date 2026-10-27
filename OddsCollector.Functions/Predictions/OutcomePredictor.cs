using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal sealed class OutcomePredictor(IScoreCalculator calculator) : IOutcomePredictor
{
    public bool TryGetOutcome(IReadOnlyCollection<Odd> odds, out OutcomeType outcome)
    {
        ArgumentNullException.ThrowIfNull(odds);

        outcome = default;

        if (odds.Count == 0)
        {
            return false;
        }

        var best = calculator.GetScores(odds)
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.Outcome)
            .First();

        if (best.Score <= 0)
        {
            return false;
        }

        outcome = best.Outcome;

        return true;
    }
}
