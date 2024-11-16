using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Processors;

internal class UpcomingEventsProcessor(IOddsApiClient client) : IUpcomingEventsProcessor
{
    public async Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync(CancellationToken cancellationToken)
    {
        return await client.GetUpcomingEventsAsync(Guid.NewGuid(), DateTime.UtcNow, cancellationToken);
    }
}
