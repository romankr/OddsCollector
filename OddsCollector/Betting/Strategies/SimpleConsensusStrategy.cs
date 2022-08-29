namespace OddsCollector.Betting.Strategies;

using Betting;
using Common;
using Models;

public class SimpleConsensusStrategy : IBettingStrategy
{
    public BettingStrategyResult Evaluate(IEnumerable<SportEvent> events)
    {
        if (events is null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        var suggestions = GetSuggestions(events).ToList();
        var statistics = GetStatistics(suggestions);

        return new BettingStrategyResult
        {
            Suggestions = suggestions,
            Statistics = statistics
        };
    }

    private static IEnumerable<BettingSuggestion> GetSuggestions(IEnumerable<SportEvent> events)
    {
        if (events is null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        var suitableEvents =
            events.Where(e => e.Odds is not null && e.Odds.Count >= 3);

        foreach (var e in suitableEvents)
        {
            var winner = GetConsensusWinner(e);
            var bestOdd = GetBestOdd(e, winner.Key);

            yield return new BettingSuggestion {
                AwayTeam = e.AwayTeam,
                HomeTeam = e.HomeTeam,
                ExpectedOutcome = winner.Key,
                RealOutcome = e.Outcome,
                SportEventId = e.SportEventId,
                BestBookmaker = bestOdd.Key,
                BestScore = bestOdd.Value,
                CommenceTime = e.CommenceTime
            };
        }
    }
        
    private static KeyValuePair<string, double> GetConsensusWinner(SportEvent sportEvent)
    {
        if (sportEvent is null)
        {
            throw new ArgumentNullException(nameof(sportEvent));
        }

        if (sportEvent.Odds is null)
        {
            throw new Exception("Odds cannot be null or empty");
        }

        if (string.IsNullOrEmpty(sportEvent.HomeTeam))
        {
            throw new Exception("HomeTeam cannot be null or empty");
        }

        if (string.IsNullOrEmpty(sportEvent.AwayTeam))
        {
            throw new Exception("AwayTeam cannot be null or empty");
        }

        var dict = new Dictionary<string, double>
        {
            { Constants.Draw, sportEvent.Odds.Select(o => o.DrawOdd).Average() },
            { sportEvent.HomeTeam, sportEvent.Odds.Select(o => o.HomeOdd).Average() },
            { sportEvent.AwayTeam, sportEvent.Odds.Select(o => o.AwayOdd).Average() }
        };

        return dict.MinBy(p => p.Value);
    }

    private static KeyValuePair<string, double> GetBestOdd(SportEvent sportEvent, string winner)
    {
        if (sportEvent is null)
        {
            throw new ArgumentNullException(nameof(sportEvent));
        }

        if (string.IsNullOrEmpty(winner))
        {
            throw new ArgumentException("winner cannot be null or empty", nameof(winner));
        }

        if (sportEvent.Odds is null)
        {
            throw new Exception("Odds cannot be null or empty");
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
            throw new Exception("Failed to find an odd");
        }

        if (string.IsNullOrEmpty(odd.Bookmaker))
        {
            throw new Exception("Bookmaker cannot be null or empty.");
        }

        return new KeyValuePair<string, double>(odd.Bookmaker, bestScore);
    }

    private static Statistics GetStatistics(IEnumerable<BettingSuggestion> suggestions)
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
            TotalNumberOfGames = completedEventsCount
        };
    }
}