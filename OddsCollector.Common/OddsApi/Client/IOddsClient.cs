using OddsCollector.Common.Models;

namespace OddsCollector.Common.OddsApi.Client;

public interface IOddsClient
{
    Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync(string league);
    Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync(IEnumerable<string> leagues);
    Task<IEnumerable<EventResult>> GetEventResultsAsync(string league);
    Task<IEnumerable<EventResult>> GetEventResultsAsync(IEnumerable<string> leagues);
}
