using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.OddsApi;

internal interface IOddsApiClient
{
    Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync(CancellationToken cancellationToken);
    Task<IEnumerable<EventResult>> GetEventResultsAsync(CancellationToken cancellationToken);
}
