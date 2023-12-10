using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Functions;

internal class EventResultsFunction(ILogger<EventResultsFunction> logger, IOddsApiClient client)
{
    [Function(nameof(EventResultsFunction))]
    [CosmosDBOutput("%CosmosDb:Database%", "%CosmosDb:EventResultsContainer%", Connection = "CosmosDb:Connection")]
    public async Task<EventResult[]> Run(
        [TimerTrigger("%EventResultsFunction:TimerInterval%")]
        CancellationToken cancellationToken)
    {
        try
        {
            return (await client.GetEventResultsAsync(Guid.NewGuid(), DateTime.UtcNow, cancellationToken)
                .ConfigureAwait(false)).ToArray();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to get event results");
        }

        return [];
    }
}
