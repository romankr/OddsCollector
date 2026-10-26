using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal sealed class OutcomePredictor(IScoreCalculator calculator) : IOutcomePredictor
{
    public OutcomeType GetOutcome(IReadOnlyCollection<Odd> odds)
    {
        ArgumentNullException.ThrowIfNull(odds);

        if (odds.Count == 0)
        {
            throw new ArgumentException($"{nameof(odds)} cannot be empty", nameof(odds));
        }

        // On equal scores the outcome declared first in OutcomeType wins (HomeTeam, then Draw, then AwayTeam),
        // so ties do not depend on the order in which the calculator returns its scores.
        var best = calculator.GetScores(odds)
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.Outcome)
            .First();

        if (best.Score <= 0)
        {
            throw new ArgumentException($"{nameof(odds)} has no usable quote", nameof(odds));
        }

        return best.Outcome;
    }
}
