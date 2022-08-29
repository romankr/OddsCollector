namespace OddsCollector.Betting.Strategies;

using Betting;
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
            ConsensusStrategyHelper.GetSuggestions(events)
                .Where(e => e.AverageScore > 1 / (e.AverageScore - 0.05))
                .ToList();

        return new BettingStrategyResult
        {
            Suggestions = suggestions,
            Statistics = ConsensusStrategyHelper.GetStatistics(suggestions)
        };
    }
}