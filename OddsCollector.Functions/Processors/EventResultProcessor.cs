using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Processors;

internal sealed class EventResultProcessor(IOddsApiClient client) : IEventResultProcessor
{
    public async Task<IEnumerable<EventResult>> GetEventResultsAsync(CancellationToken cancellationToken)
    {
        return await client.GetEventResultsAsync(cancellationToken);
    }
}
