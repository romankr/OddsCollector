using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Processors;

internal sealed class EventResultProcessor(ILogger<EventResultProcessor> logger, IEventResultsClient client)
    : IEventResultProcessor
{
    public async Task<EventResult[]> GetEventResultsAsync(CancellationToken cancellationToken)
    {
        var result = await client.GetEventResultsAsync(cancellationToken);

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
