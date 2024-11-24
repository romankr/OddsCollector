using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.OddsApi.Clients;

internal interface IUpcomingEventsClient
{
    IAsyncEnumerable<UpcomingEvent> GetUpcomingEventsAsync(Guid traceId, DateTime timestamp, CancellationToken cancellationToken);
}
