namespace OddsCollector.Prediction;

using Common;
using Models;

public class Predictor : IPredictor
{
    public IEnumerable<Prediction> GetPredictions(IEnumerable<SportEvent> events)
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

            yield return new Prediction {
                AwayTeam = e.AwayTeam,
                HomeTeam = e.HomeTeam,
                ExpectedOutcome = winner.Key,
                RealOutcome = e.Outcome,
                SportEventId = e.SportEventId,
                BestBookmaker = bestOdd.Key,
                BestScore = bestOdd.Value
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
            { Constants.Draw,
                GetConsensusProbability(sportEvent.Odds.Select(o => o.DrawOdd))},

            { sportEvent.HomeTeam,
                GetConsensusProbability(sportEvent.Odds.Select(o => o.HomeOdd))},

            { sportEvent.AwayTeam,
                GetConsensusProbability(sportEvent.Odds.Select(o => o.AwayOdd))}
        };

        return dict.MaxBy(p => p.Value);
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

    private static double GetConsensusProbability(IEnumerable<double> odds)
    {
        if (odds is null)
        {
            throw new ArgumentNullException(nameof(odds));
        }

        var average = odds.Average();

        return average == 0 ? 0 : 1 / average;
    }

    public Statistics GetStatistics(IEnumerable<Prediction> predictions)
    {
        if (predictions is null)
        {
            throw new ArgumentNullException(nameof(predictions));
        }

        var computedPredictions = predictions.ToList();
            
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