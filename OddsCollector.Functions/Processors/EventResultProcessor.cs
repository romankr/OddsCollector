using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Processors;

internal class EventResultProcessor(IOddsApiClient client) : IEventResultProcessor
{
    public async Task<IEnumerable<EventResult>> GetEventResultsAsync(CancellationToken cancellationToken)
    {
        return await client.GetEventResultsAsync(cancellationToken);
    }
}
