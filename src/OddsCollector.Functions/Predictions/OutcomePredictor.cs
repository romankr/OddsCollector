using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal sealed class OutcomePredictor(IScoreCalculator calculator) : IOutcomePredictor
{
    public OutcomeResult GetOutcome(IReadOnlyCollection<Odd> odds)
    {
        ArgumentNullException.ThrowIfNull(odds);

        if (odds.Count == 0)
        {
            return new NoUsableOdds();
        }

        var best = calculator.GetScores(odds)
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.Outcome)
            .First();

        if (best.Score <= 0)
        {
            return new NoUsableOdds();
        }

        return best.Outcome;
    }
}
