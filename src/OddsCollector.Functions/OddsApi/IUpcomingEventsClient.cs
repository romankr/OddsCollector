using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.OddsApi;

internal interface IUpcomingEventsClient
{
    Task<UpcomingEvent[]> GetUpcomingEventsAsync(CancellationToken cancellationToken);
}
