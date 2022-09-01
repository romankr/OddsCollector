namespace OddsCollector.Csv;

using Betting;

public interface ICsvSaver
{
    Task WriteBettingStrategyResultAsync(string bettingStrategyName, BettingStrategyResult result);
}