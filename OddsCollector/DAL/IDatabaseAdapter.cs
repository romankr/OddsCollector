namespace OddsCollector.DAL;

using Models;

public interface IDatabaseAdapter
{
    Task SaveUpcomingEventsAsync(IEnumerable<SportEvent> events);

    Task SaveEventResultsAsync(Dictionary<string, string?> results);

    IEnumerable<SportEvent> GetEventsWithLatestOdds();
}