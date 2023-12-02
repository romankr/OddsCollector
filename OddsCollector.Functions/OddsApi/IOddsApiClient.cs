using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.OddsApi;

public interface IOddsApiClient
{
    Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync(Guid traceId, DateTime timestamp, CancellationToken cancellationToken);
    Task<IEnumerable<EventResult>> GetEventResultsAsync(Guid traceId, DateTime timestamp, CancellationToken cancellationToken);
}
