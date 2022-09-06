namespace OddsCollector.Csv;

using Betting;

/// <summary>
/// Basic interface for classes that write betting strategy suggestions and effectiveness to CSV files.
/// </summary>
public interface ICsvSaver
{
    /// <summary>
    /// Writes betting strategy suggestions and effectiveness to CSV files.
    /// </summary>
    /// <param name="bettingStrategyName">A betting strategy name.</param>
    /// <param name="result">Betting strategy suggestions and effectiveness in <see cref="BettingStrategyResult"/>.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    Task WriteBettingStrategyResultAsync(string bettingStrategyName, BettingStrategyResult result);
}