using OddsCollector.Common.ExchangeContracts;

namespace OddsCollector.Service.Prediction.Strategies;

/// <remarks>
///     Based on this article
///     https://www.researchgate.net/publication/320296375_Beating_the_bookies_with_their_own_numbers_-_and_how_the_online_sports_betting_market_is_rigged.
/// </remarks>
internal sealed class AdjustedConsensusStrategy : SimpleConsensusStrategy
{
    protected override string PredictWinner(IEnumerable<Odd?>? odds, string? awayTeam, string? homeTeam)
    {
        if (odds is null)
        {
            throw new ArgumentNullException(nameof(odds));
        }

        if (string.IsNullOrEmpty(awayTeam))
        {
            throw new ArgumentOutOfRangeException(nameof(awayTeam), $"{nameof(awayTeam)} is null or empty");
        }

        if (string.IsNullOrEmpty(homeTeam))
        {
            throw new ArgumentOutOfRangeException(nameof(homeTeam), $"{nameof(homeTeam)} is null or empty");
        }

        return CalculateScores(odds.ToList(), awayTeam, homeTeam).MaxBy(p => p.Value).Key;
    }

    protected override Dictionary<string, double?> CalculateScores(IEnumerable<Odd?> odds, string awayTeam,
        string homeTeam)
    {
        var enumeratedOdds = odds.ToList();

        return new Dictionary<string, double?>
        {
            { Constants.Draw, CalculateAdjustedScore(enumeratedOdds, OutcomeFilters.Draw, 0.057) },
            { awayTeam, CalculateAdjustedScore(enumeratedOdds, OutcomeFilters.AwayTeamWins, 0.034) },
            { homeTeam, CalculateAdjustedScore(enumeratedOdds, OutcomeFilters.HomeTeamWins, 0.037) }
        };
    }

    private static double? CalculateAdjustedScore(IEnumerable<Odd?> odds, Func<Odd?, double?>? filter,
        double adjustment)
    {
        if (filter is null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        var average = odds.Select(filter).Average();
        return average == 0 ? 0 : (1 / average) - adjustment;
    }
}
