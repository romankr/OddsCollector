using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Processors;

internal sealed class UpcomingEventsProcessor(ILogger<UpcomingEventsProcessor> logger, IOddsApiClient client) : IUpcomingEventsProcessor
{
    public async Task<UpcomingEvent[]> GetUpcomingEventsAsync(CancellationToken cancellationToken)
    {
        var result = (await client.GetUpcomingEventsAsync(cancellationToken)).ToArray();

        if (result.Length == 0)
        {
            logger.LogWarning("No events received");
        }
        else
        {
            logger.LogInformation("{Length} event(s) received", result.Length);
        }

        return result;
    }
}
