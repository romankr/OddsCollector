namespace OddsCollector.Betting;

/// <summary>
/// Stores results of a betting strategy evaluation.
/// </summary>
public class BettingStrategyResult
{
    /// <summary>
    /// List of suggestions.
    /// </summary>
    public IEnumerable<BettingSuggestion>? Suggestions { get; set; }

    /// <summary>
    /// Effectiveness of the betting strategy. 
    /// </summary>
    public Statistics? Statistics { get; set; }
}