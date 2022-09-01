namespace OddsCollector.Csv;

using Betting;

public interface ICsvSaver
{
    Task WriteBettingStrategyResultAsync(string? dir, string bettingStrategyName, BettingStrategyResult result);
}