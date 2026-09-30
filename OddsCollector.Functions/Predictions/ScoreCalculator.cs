using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal sealed class ScoreCalculator : IScoreCalculator
{
    private const double MinimumDecimalOdds = 1;

    // The regression intercepts from the paper cited on PredictionStrategy: the actual
    // probability of an outcome is roughly the bookmakers' consensus minus these.
    private const double DrawAdjustment = 0.057;
    private const double HomeTeamAdjustment = 0.034;
    private const double AwayTeamAdjustment = 0.037;

    public OutcomeScore[] GetScores(ICollection<Odd> odds)
    {
        return
        [
            Calculate(OutcomeTypes.Draw, odds, DrawAdjustment, x => x.Draw),
            Calculate(OutcomeTypes.AwayTeam, odds, AwayTeamAdjustment, x => x.Away),
            Calculate(OutcomeTypes.HomeTeam, odds, HomeTeamAdjustment, x => x.Home)
        ];
    }

    private static OutcomeScore Calculate(string outcome, ICollection<Odd> odds, double adjustment,
        Func<Odd, double> valueExtractor)
    {
        var probabilities = GetImpliedProbabilities(odds, valueExtractor);

        var score = probabilities.Count == 0 ? 0 : probabilities.Average() - adjustment;

        return ToOutcomeScore(outcome, score);
    }

    private static List<double> GetImpliedProbabilities(ICollection<Odd> odds, Func<Odd, double> valueExtractor)
    {
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
