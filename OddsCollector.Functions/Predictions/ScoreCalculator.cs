using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal sealed class ScoreCalculator : IScoreCalculator
{
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
        var score = GetConsensusScore(odds, valueExtractor);
        score = AdjustScore(score, adjustment);
        return ToOutcomeScore(outcome, score);
    }

    private static double GetConsensusScore(ICollection<Odd> odds, Func<Odd, double> valueExtractor)
    {
        return odds.Select(valueExtractor).Average();
    }

    private static double AdjustScore(double score, double adjustment)
    {
        return score == 0 ? 0 : (1 / score) - adjustment;
    }

    private static OutcomeScore ToOutcomeScore(string outcome, double score)
    {
        return new OutcomeScoreBuilder()
            .SetOutcome(outcome)
            .SetScore(score)
            .Instance;
    }
}
