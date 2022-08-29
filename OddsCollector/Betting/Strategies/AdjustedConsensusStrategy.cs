namespace OddsCollector.Betting.Strategies;

using Betting;
using Common;
using Models;

public class AdjustedConsensusStrategy : IBettingStrategy
{
    public BettingStrategyResult Evaluate(IEnumerable<SportEvent> events)
    {
        if (events is null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        var suggestions =
            GetSuggestions(events).ToList();

        return new BettingStrategyResult
        {
            Suggestions = suggestions,
            Statistics = ConsensusStrategyHelper.GetStatistics(suggestions)
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
            var bestOdd = ConsensusStrategyHelper.GetBestOdd(e, winner.Key);

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
            { Constants.Draw, GetConsensusProbability(sportEvent.Odds.Select(o => o.DrawOdd), 0.057)},
            { sportEvent.HomeTeam, GetConsensusProbability(sportEvent.Odds.Select(o => o.HomeOdd), 0.034)},
            { sportEvent.AwayTeam, GetConsensusProbability(sportEvent.Odds.Select(o => o.AwayOdd), 0.037)}
        };

        return dict.MaxBy(p => p.Value);
    }

    private static double GetConsensusProbability(IEnumerable<double> odds, double adjustment)
    {
        if (odds is null)
        {
            throw new ArgumentNullException(nameof(odds));
        }

        if (adjustment == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(adjustment), "Adjustment cannot be 0");
        }

        var average = odds.Average();

        return average == 0 ? 0 : (1 / average) - adjustment;
    }
}