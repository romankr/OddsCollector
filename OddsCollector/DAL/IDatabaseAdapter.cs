namespace OddsCollector.DAL;

using Models;

public interface IDatabaseAdapter
{
    void SaveUpcomingEvents(IEnumerable<SportEvent> events);

    void SaveEventResults(Dictionary<string, string?> results);

    IEnumerable<SportEvent> GetEventsWithLatestOdds();
}