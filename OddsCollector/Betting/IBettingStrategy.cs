namespace OddsCollector.Betting;

using Models;

public interface IBettingStrategy
{
    BettingStrategyResult Evaluate(IEnumerable<SportEvent> events);

    string Name { get; }
}