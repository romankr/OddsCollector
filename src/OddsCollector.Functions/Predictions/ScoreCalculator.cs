using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal sealed class ScoreCalculator : IScoreCalculator
{
    private const double BreakEvenDecimalOdds = 1;

    private const double DrawAdjustment = 0.057;
    private const double HomeTeamAdjustment = 0.034;
    private const double AwayTeamAdjustment = 0.037;

    public OutcomeScore[] GetScores(IReadOnlyCollection<Odd> odds)
    {
        ArgumentNullException.ThrowIfNull(odds);

        List<Odd> usableOdds = [with(capacity: odds.Count), .. odds.Where(IsUsable)];

        return
        [
            Calculate(OutcomeType.Draw, usableOdds, DrawAdjustment, x => x.Draw),
            Calculate(OutcomeType.AwayTeam, usableOdds, AwayTeamAdjustment, x => x.Away),
            Calculate(OutcomeType.HomeTeam, usableOdds, HomeTeamAdjustment, x => x.Home)
        ];
    }

    private static OutcomeScore Calculate(OutcomeType outcome, List<Odd> usableOdds, double adjustment,
        Func<Odd, double> valueExtractor)
    {
        var score = usableOdds.Count == 0 ? 0 : usableOdds.Average(o => 1 / valueExtractor(o)) - adjustment;

        return new OutcomeScore { Outcome = outcome, Score = score };
    }

    private static bool IsUsable(Odd odd)
    {
        return IsUsablePrice(odd.Home) && IsUsablePrice(odd.Draw) && IsUsablePrice(odd.Away);
    }

    private static bool IsUsablePrice(double price)
    {
        return double.IsFinite(price) && price > BreakEvenDecimalOdds;
    }
}
