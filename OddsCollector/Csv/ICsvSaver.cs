namespace OddsCollector.Csv;

using Betting;

public interface ICsvSaver
{
    void WriteBettingStrategyResult(string? dir, string bettingStrategyName, BettingStrategyResult result);
}