namespace OddsCollector.Betting.Strategies;

using Betting;
using Models;

public class SimpleConsensusStrategy : IBettingStrategy
{
    public BettingStrategyResult Evaluate(IEnumerable<SportEvent> events)
    {
        if (events is null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        var suggestions = 
            ConsensusStrategyHelper.GetSuggestions(events).ToList();

        return new BettingStrategyResult
        {
            Suggestions = suggestions,
            Statistics = ConsensusStrategyHelper.GetStatistics(suggestions)
        };
    }
}