using OddsCollector.Common.Models;

namespace OddsCollector.Common.OddsApi;

public interface IOddsApiClient
{
    Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync(Guid traceId, DateTime timestamp, CancellationToken cancellationToken);
    Task<IEnumerable<EventResult>> GetEventResultsAsync(Guid traceId, DateTime timestamp, CancellationToken cancellationToken);
}
