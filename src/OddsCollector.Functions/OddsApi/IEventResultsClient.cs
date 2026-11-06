using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.OddsApi;

internal interface IEventResultsClient
{
    Task<EventResult[]> GetEventResultsAsync(CancellationToken cancellationToken);
}
