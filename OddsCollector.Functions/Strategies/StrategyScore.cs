namespace OddsCollector.Functions.Strategies;

internal class StrategyScore
{
    public string Bookmaker { get; set; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public double Odd { get; init; }
}
