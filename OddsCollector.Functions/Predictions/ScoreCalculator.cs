using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal sealed class ScoreCalculator : IScoreCalculator
{
    private const double MinimumDecimalOdds = 1;

    public OutcomeScore[] GetScores(ICollection<Odd> odds)
    {
        return
        [
            Calculate(OutcomeTypes.Draw, odds, 0.057, x => x.Draw),
            Calculate(OutcomeTypes.AwayTeam, odds, 0.034, x => x.Away),
            Calculate(OutcomeTypes.HomeTeam, odds, 0.037, x => x.Home)
        ];
    }

    private static OutcomeScore Calculate(string outcome, ICollection<Odd> odds, double adjustment,
        Func<Odd, double> valueExtractor)
    {
        var probabilities = GetImpliedProbabilities(odds, valueExtractor);

        // Without a usable quote there is nothing to score, and taking the adjustment off
        // nothing would leave a negative one.
        var score = probabilities.Count == 0 ? 0 : probabilities.Average() - adjustment;

        return ToOutcomeScore(outcome, score);
    }

    /// <remarks>
    ///     The consensus is the mean of what each bookmaker implies, not what the mean of
    ///     their odds implies. 1 / mean(odds) is not mean(1 / odds), and the two diverge the
    ///     more the bookmakers disagree, which held contested outcomes down.
    /// </remarks>
    private static List<double> GetImpliedProbabilities(ICollection<Odd> odds, Func<Odd, double> valueExtractor)
    {
        // Decimal odds never fall below 1, so a quote under it is unusable. It is left out
        // rather than averaged in as a zero, which would drag the consensus down.
        return odds.Select(valueExtractor)
            .Where(o => o >= MinimumDecimalOdds)
            .Select(o => 1 / o)
            .ToList();
    }

    private static OutcomeScore ToOutcomeScore(string outcome, double score)
    {
        return new OutcomeScoreBuilder()
            .SetOutcome(outcome)
            .SetScore(score)
            .Instance;
    }
}
