using System.Runtime.CompilerServices;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Clients;

namespace OddsCollector.Functions.Processors;

internal class EventResultProcessor(IEventResultsClient client) : IEventResultProcessor
{
    public async IAsyncEnumerable<EventResult> GetEventResultsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        return await client.GetEventResultsAsync(Guid.NewGuid(), DateTime.UtcNow, cancellationToken);
    }
}
