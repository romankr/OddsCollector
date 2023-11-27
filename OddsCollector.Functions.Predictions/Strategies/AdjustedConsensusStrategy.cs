using OddsCollector.Common.Models;

namespace OddsCollector.Functions.Predictions.Strategies;

/// <remarks>
///     Based on this article
///     https://www.researchgate.net/publication/320296375_Beating_the_bookies_with_their_own_numbers_-_and_how_the_online_sports_betting_market_is_rigged.
/// </remarks>
internal sealed class AdjustedConsensusStrategy : IPredictionStrategy
{
    public EventPrediction GetPrediction(UpcomingEvent? upcomingEvent, DateTime? timestamp)
    {
        ArgumentNullException.ThrowIfNull(upcomingEvent);
        ArgumentNullException.ThrowIfNull(timestamp);

        var score = GetWinner(upcomingEvent.Odds, upcomingEvent.AwayTeam, upcomingEvent.HomeTeam);

        return new EventPredictionBuilder()
            .SetAwayTeam(upcomingEvent.AwayTeam)
            .SetHomeTeam(upcomingEvent.HomeTeam)
            .SetCommenceTime(upcomingEvent.CommenceTime)
            .SetStrategy(nameof(AdjustedConsensusStrategy))
            .SetId(upcomingEvent.Id)
            .SetTimestamp(timestamp)
            .SetTraceId(upcomingEvent.TraceId)
            .SetWinner(score.Name)
            .SetBookmaker(score.Bookmaker)
            .Instance;
    }

    private static StrategyScore GetWinner(IEnumerable<Odd> odds, string awayTeam, string homeTeam)
    {
        var enumerated = odds.ToList();

        if (enumerated.Count == 0)
        {
            throw new ArgumentException($"{nameof(odds)} cannot be empty", nameof(odds));
        }

        ArgumentException.ThrowIfNullOrEmpty(awayTeam);
        ArgumentException.ThrowIfNullOrEmpty(homeTeam);

        List<StrategyScore> scores = [
            new() { Name = Constants.Draw, Odd = CalculateAdjustedScore(enumerated, Draw, 0.057) },
            new() { Name = awayTeam, Odd = CalculateAdjustedScore(enumerated, AwayTeamWins, 0.034) },
            new() { Name = homeTeam, Odd = CalculateAdjustedScore(enumerated, HomeTeamWins, 0.037) }
        ];

        var winner = scores.MaxBy(p => p.Odd);

        winner!.Bookmaker = GetBestOdd(enumerated, winner.Name, awayTeam, homeTeam).Bookmaker;

        return winner;
    }

    private static double CalculateAdjustedScore(IEnumerable<Odd> odds, Func<Odd, double> filter, double adjustment)
    {
        var average = odds.Select(filter).Average();
        return average == 0 ? 0 : (1 / average) - adjustment;
    }

    private static Odd GetBestOdd(IEnumerable<Odd> odds, string winner, string awayTeam, string homeTeam)
    {
        var enumerated = odds.ToList();

        IEnumerable<Odd> filtered = [];

        if (winner == Constants.Draw)
        {
            filtered = enumerated.OrderByDescending(Draw);
        }

        if (winner == awayTeam)
        {
            filtered = enumerated.OrderByDescending(AwayTeamWins);
        }

        if (winner == homeTeam)
        {
            filtered = enumerated.OrderByDescending(HomeTeamWins);
        }

        return filtered.FirstOrDefault()!;
    }

    private static double Draw(Odd odd)
    {
        return odd.Draw;
    }

    private static double AwayTeamWins(Odd odd)
    {
        return odd.Away;
    }

    private static double HomeTeamWins(Odd odd)
    {
        return odd.Home;
    }
}
