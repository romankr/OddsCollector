namespace OddsCollector.Betting.Strategies;

using Betting;
using Common;
using Models;

/// <summary>
/// Strategy that simply calculates consensus outcome from multiple bookmakers.
/// </summary>
public class SimpleConsensusStrategy : IBettingStrategy
{
    /// <summary>
    /// Calculates suggestions and effectiveness of the strategy. 
    /// </summary>
    /// <param name="events">A list of <see cref="SportEvent"/> to analyze.</param>
    /// <returns>Suggestions and effectiveness of the strategy in <see cref="BettingStrategyResult"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
    public virtual BettingStrategyResult Evaluate(IEnumerable<SportEvent> events)
    {
        if (events is null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        var suggestions = GetSuggestions(events).ToList();

        return new BettingStrategyResult
        {
            Suggestions = suggestions,
            Statistics = GetStatistics(suggestions)
        };
    }

    /// <summary>
    /// Calculates suggestions to be yielded by the strategy.
    /// </summary>
    /// <param name="events">A list of <see cref="SportEvent"/> to analyze.</param>
    /// <returns>A list of <see cref="BettingSuggestion"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
    protected virtual IEnumerable<BettingSuggestion> GetSuggestions(IEnumerable<SportEvent> events)
    {
        if (events is null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        foreach (var e in FilterInitialEvents(events))
        {
            var winner = GetConsensusWinner(e);
            var bestOdd = GetBestOdd(e, winner.Key);

            yield return new BettingSuggestion
            {
                AwayTeam = e.AwayTeam,
                HomeTeam = e.HomeTeam,
                ExpectedOutcome = winner.Key,
                RealOutcome = e.Outcome,
                SportEventId = e.SportEventId,
                BestBookmaker = bestOdd.Key,
                AverageScore = winner.Value,
                BestScore = bestOdd.Value,
                CommenceTime = e.CommenceTime
            };
        }
    }

    /// <summary>
    /// Filters events before making any suggestions.
    /// By default any <see cref="SportEvent"/> that has less than 3 odds is being discarded.
    /// </summary>
    /// <param name="events">A list of <see cref="SportEvent"/> to filter.</param>
    /// <returns>A filtered list of <see cref="SportEvent"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
    protected virtual IEnumerable<SportEvent> FilterInitialEvents(IEnumerable<SportEvent> events)
    {
        if (events is null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        return events.Where(e => e.Odds is not null && e.Odds.Count >= 3);
    }

    /// <summary>
    /// Gets consensus winner and average score for given event.
    /// </summary>
    /// <param name="sportEvent"><see cref="SportEvent"/> to analyze.</param>
    /// <returns>A <see cref="KeyValuePair"/> of winner and average score.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sportEvent"/> is null.</exception>
    /// <exception cref="Exception">
    /// Odds cannot be null or empty or
    /// HomeTeam cannot be null or empty or
    /// AwayTeam cannot be null or empty.
    /// </exception>
    protected virtual KeyValuePair<string, double> GetConsensusWinner(SportEvent sportEvent)
    {
        if (sportEvent is null)
        {
            throw new ArgumentNullException(nameof(sportEvent));
        }

        if (sportEvent.Odds is null)
        {
            throw new Exception("Odds cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(sportEvent.HomeTeam))
        {
            throw new Exception("HomeTeam cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(sportEvent.AwayTeam))
        {
            throw new Exception("AwayTeam cannot be null or empty.");
        }

        var dict = new Dictionary<string, double>
        {
            { Constants.Draw, sportEvent.Odds.Select(o => o.DrawOdd).Average() },
            { sportEvent.HomeTeam, sportEvent.Odds.Select(o => o.HomeOdd).Average() },
            { sportEvent.AwayTeam, sportEvent.Odds.Select(o => o.AwayOdd).Average() }
        };

        return dict.MinBy(p => p.Value);
    }

    /// <summary>
    /// Gets best score and bookmaker for given event.
    /// </summary>
    /// <param name="sportEvent">A <see cref="SportEvent"/> instance.</param>
    /// <param name="winner">A winner.</param>
    /// <returns>A <see cref="KeyValuePair"/> of bookmaker and score.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sportEvent"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="winner"/> cannot be null or empty.</exception>
    /// <exception cref="Exception">
    /// Odds cannot be null or empty or
    /// Failed to find an odd or
    /// Bookmaker cannot be null or empty.
    /// </exception>
    protected virtual KeyValuePair<string, double> GetBestOdd(SportEvent sportEvent, string winner)
    {
        if (sportEvent is null)
        {
            throw new ArgumentNullException(nameof(sportEvent));
        }

        if (string.IsNullOrEmpty(winner))
        {
            throw new ArgumentException("Winner cannot be null or empty.", nameof(winner));
        }

        if (sportEvent.Odds is null)
        {
            throw new Exception("Odds cannot be null or empty.");
        }

        var bestScore = 0.0;
        Odd? odd = null;

        if (winner == Constants.Draw)
        {
            odd = sportEvent.Odds.OrderByDescending(o => o.DrawOdd).First();
            bestScore = odd.DrawOdd;
        }

        if (winner == sportEvent.AwayTeam)
        {
            odd = sportEvent.Odds.OrderByDescending(o => o.AwayOdd).First();
            bestScore = odd.AwayOdd;
        }

        if (winner == sportEvent.HomeTeam)
        {
            odd = sportEvent.Odds.OrderByDescending(o => o.HomeOdd).First();
            bestScore = odd.HomeOdd;
        }

        if (odd is null)
        {
            throw new Exception("Failed to find an odd.");
        }

        if (string.IsNullOrEmpty(odd.Bookmaker))
        {
            throw new Exception("Bookmaker cannot be null or empty.");
        }

        return new KeyValuePair<string, double>(odd.Bookmaker, bestScore);
    }

    /// <summary>
    /// Calculates effectiveness of the strategy.
    /// </summary>
    /// <param name="suggestions">List of <see cref="BettingSuggestion"/>.</param>
    /// <returns>A <see cref="Statistics"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="suggestions"/> is null.</exception>
    protected virtual Statistics GetStatistics(IEnumerable<BettingSuggestion> suggestions)
    {
        if (suggestions is null)
        {
            throw new ArgumentNullException(nameof(suggestions));
        }

        var computedPredictions = suggestions.ToList();

        var completedEventsCount =
            computedPredictions.Count(p => p.RealOutcome is not null);

        if (completedEventsCount == 0)
        {
            return new Statistics();
        }

        var successfulPredictions =
            computedPredictions
                .Where(p => p.RealOutcome is not null && p.RealOutcome == p.ExpectedOutcome)
                .ToList();

        var successfulPredictionsCount = successfulPredictions.Count;

        var potentialIncome =
            successfulPredictions.Where(p => p.RealOutcome is not null)
                .Select(p => p.BestScore)
                .Sum();

        var failedPredictionsCount = completedEventsCount - successfulPredictionsCount;

        var earningsInPoints = potentialIncome - failedPredictionsCount - successfulPredictionsCount;

        return new Statistics
        {
            Accuracy = (double)successfulPredictionsCount / completedEventsCount,
            EarningsInPoints = earningsInPoints,
            Earnings10Bet = earningsInPoints * 10,
            Earnings20Bet = earningsInPoints * 20,
            Earnings50Bet = earningsInPoints * 50,
            NumberOfSuccessfulPredictions = successfulPredictionsCount,
            TotalNumberOfEvents = completedEventsCount
        };
    }
}