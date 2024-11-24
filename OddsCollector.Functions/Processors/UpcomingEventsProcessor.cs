using System.Runtime.CompilerServices;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Clients;

namespace OddsCollector.Functions.Processors;

internal class UpcomingEventsProcessor(IUpcomingEventsClient client) : IUpcomingEventsProcessor
{
    public async IAsyncEnumerable<UpcomingEvent> GetUpcomingEventsAsync([EnumeratorCancellation]CancellationToken cancellationToken)
    {
        return await client.GetUpcomingEventsAsync(Guid.NewGuid(), DateTime.UtcNow, cancellationToken);
    }
}
