using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.OddsApi.Clients;

internal interface IEventResultsClient
{
    IAsyncEnumerable<EventResult> GetEventResultsAsync(Guid traceId, DateTime timestamp, CancellationToken cancellationToken);
}
