namespace OddsCollector.Api.GoogleApi;

using Betting;

public interface IGoogleApiAdapter
{
    void CreateReport(string bettingStrategyName, BettingStrategyResult? result);
}