using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Processors;

internal sealed class UpcomingEventsProcessor(IOddsApiClient client) : IUpcomingEventsProcessor
{
    public async Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync(CancellationToken cancellationToken)
    {
        return await client.GetUpcomingEventsAsync(cancellationToken);
    }
}
