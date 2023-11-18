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
        if (upcomingEvent is null)
        {
            throw new ArgumentNullException(nameof(upcomingEvent));
        }

        if (timestamp is null)
        {
            throw new ArgumentNullException(nameof(timestamp));
        }

        return MakePrediction(upcomingEvent, timestamp.Value);
    }

    private static EventPrediction MakePrediction(UpcomingEvent upcomingEvent, DateTime timestamp)
    {
        var winner = PredictWinner(upcomingEvent.Odds, upcomingEvent.AwayTeam, upcomingEvent.HomeTeam);
        var odd = GetBestOdd(upcomingEvent.Odds, winner, upcomingEvent.AwayTeam, upcomingEvent.HomeTeam);

        return new EventPrediction
        {
            Id = upcomingEvent.Id,
            Timestamp = timestamp,
            TraceId = upcomingEvent.TraceId,
            Winner = winner,
            Bookmaker = odd?.Bookmaker,
            Strategy = nameof(AdjustedConsensusStrategy),
            CommenceTime = upcomingEvent?.CommenceTime,
            AwayTeam = upcomingEvent?.AwayTeam,
            HomeTeam = upcomingEvent?.HomeTeam
        };
    }

    private static string PredictWinner(IEnumerable<Odd?>? odds, string? awayTeam, string? homeTeam)
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

        return CalculateScores(odds, awayTeam, homeTeam).MaxBy(p => p.Value).Key;
    }

    private static Dictionary<string, double?> CalculateScores(IEnumerable<Odd?> odds, string awayTeam, string homeTeam)
    {
        var enumeratedOdds = odds.ToList();

        return new Dictionary<string, double?>
        {
            { Constants.Draw, CalculateAdjustedScore(enumeratedOdds, Draw, 0.057) },
            { awayTeam, CalculateAdjustedScore(enumeratedOdds, AwayTeamWins, 0.034) },
            { homeTeam, CalculateAdjustedScore(enumeratedOdds, HomeTeamWins, 0.037) }
        };
    }

    private static double? CalculateAdjustedScore(IEnumerable<Odd?> odds, Func<Odd?, double?> filter, double adjustment)
    {
        var average = odds.Select(filter).Average();
        return average == 0 ? 0 : (1 / average) - adjustment;
    }

    private static Odd? GetBestOdd(IEnumerable<Odd?>? odds, string winner, string? awayTeam, string? homeTeam)
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

        if (winner == Constants.Draw)
        {
            return GetBestOdd(odds, Draw);
        }

        return winner == awayTeam ? GetBestOdd(odds, AwayTeamWins) : GetBestOdd(odds, HomeTeamWins);
    }

    private static Odd? GetBestOdd(IEnumerable<Odd?> odds, Func<Odd?, double?> filter)
    {
        return odds.OrderByDescending(filter).First();
    }

    private static double? Draw(Odd? odd)
    {
        if (odd is null)
        {
            throw new ArgumentNullException(nameof(odd));
        }

        return odd.Draw;
    }

    private static double? AwayTeamWins(Odd? odd)
    {
        if (odd is null)
        {
            throw new ArgumentNullException(nameof(odd));
        }

        return odd.Away;
    }

    private static double? HomeTeamWins(Odd? odd)
    {
        if (odd is null)
        {
            throw new ArgumentNullException(nameof(odd));
        }

        return odd.Home;
    }
}
