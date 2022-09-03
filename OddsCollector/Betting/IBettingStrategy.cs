namespace OddsCollector.Betting;

using Models;

/// <summary>
/// Basic interface for betting strategies.
/// </summary>
public interface IBettingStrategy
{
    /// <summary>
    /// Calculates suggestions and effectiveness of the strategy. 
    /// </summary>
    /// <param name="events">A list of <see cref="SportEvent"/> to analyze.</param>
    /// <returns>Suggestions and effectiveness of the strategy in <see cref="BettingStrategyResult"/>.</returns>
    BettingStrategyResult Evaluate(IEnumerable<SportEvent> events);
}