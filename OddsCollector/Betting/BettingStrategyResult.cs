namespace OddsCollector.Betting;

public class BettingStrategyResult
{
    public IEnumerable<BettingSuggestion>? Suggestions { get; set; }

    public Statistics? Statistics { get; set; }
}