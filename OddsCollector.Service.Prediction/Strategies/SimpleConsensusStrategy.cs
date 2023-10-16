using OddsCollector.Common.ExchangeContracts;

namespace OddsCollector.Service.Prediction.Strategies;

internal class SimpleConsensusStrategy : IPredictionStrategy
{
    public virtual IEnumerable<EventPrediction> GetPredictions(IEnumerable<UpcomingEvent?>? upcomingEvents,
        DateTime? timestamp)
    {
        if (upcomingEvents is null)
        {
            throw new ArgumentNullException(nameof(upcomingEvents));
        }

        if (timestamp is null)
        {
            throw new ArgumentNullException(nameof(timestamp));
        }

        return upcomingEvents.Select(e => MakePrediction(e, timestamp.Value));
    }

    protected virtual EventPrediction MakePrediction(UpcomingEvent? upcomingEvent, DateTime timestamp)
    {
        if (upcomingEvent is null)
        {
            throw new ArgumentNullException(nameof(upcomingEvent));
        }

        var winner = PredictWinner(upcomingEvent.Odds, upcomingEvent.AwayTeam, upcomingEvent.HomeTeam);
        var odd = GetBestOdd(upcomingEvent.Odds, winner, upcomingEvent.AwayTeam, upcomingEvent.HomeTeam);

        return new EventPrediction
        {
            Id = upcomingEvent.Id,
            Timestamp = timestamp,
            TraceId = upcomingEvent.TraceId,
            Winner = winner,
            BestOdd = odd,
            Strategy = GetType().Name
        };
    }

    protected virtual string PredictWinner(IEnumerable<Odd?>? odds, string? awayTeam, string? homeTeam)
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

        return CalculateScores(odds, awayTeam, homeTeam).MinBy(p => p.Value).Key;
    }

    protected virtual Dictionary<string, double?> CalculateScores(IEnumerable<Odd?> odds, string awayTeam,
        string homeTeam)
    {
        var enumeratedOdds = odds.ToList();

        return new Dictionary<string, double?>
        {
            { Constants.Draw, CalculateScore(enumeratedOdds, OutcomeFilters.Draw) },
            { awayTeam, CalculateScore(enumeratedOdds, OutcomeFilters.AwayTeamWins) },
            { homeTeam, CalculateScore(enumeratedOdds, OutcomeFilters.HomeTeamWins) }
        };
    }

    protected virtual double? CalculateScore(IEnumerable<Odd?> odds, Func<Odd?, double?>? filter)
    {
        if (filter is null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        return odds.Select(filter).Average();
    }

    protected virtual Odd? GetBestOdd(IEnumerable<Odd?>? odds, string winner, string? awayTeam, string? homeTeam)
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
            return GetBestOdd(odds, OutcomeFilters.Draw);
        }

        return winner == awayTeam
            ? GetBestOdd(odds, OutcomeFilters.AwayTeamWins)
            : GetBestOdd(odds, OutcomeFilters.HomeTeamWins);
    }

    protected virtual Odd? GetBestOdd(IEnumerable<Odd?> odds, Func<Odd?, double?>? filter)
    {
        if (filter is null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        return odds.OrderByDescending(filter).First();
    }
}
