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

        // On equal scores the outcome declared first in OutcomeType wins (HomeTeam, then Draw, then AwayTeam),
        // so ties do not depend on the order in which the calculator returns its scores.
        var best = calculator.GetScores(odds)
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.Outcome)
            .First();

        // A score of zero or less means no quote was usable: predicting from it would be a guess.
        if (best.Score <= 0)
        {
            return false;
        }

        outcome = best.Outcome;

        return true;
    }
}
