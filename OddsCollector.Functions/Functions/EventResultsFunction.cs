using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Functions;

internal sealed class EventResultsFunction(ILogger<EventResultsFunction> logger, IEventResultsClient client)
{
    [Function(nameof(EventResultsFunction))]
    [CosmosDBOutput("%CosmosDb:Database%", "%CosmosDb:EventResultsContainer%",
        Connection = "CosmosDb:Connection")]
    public async Task<EventResult[]> Run(
        [TimerTrigger("%EventResultsFunction:TimerInterval%")]
        CancellationToken cancellationToken)
    {
        try
        {
            var results = await client.GetEventResultsAsync(cancellationToken);

            if (results.Length == 0)
            {
                logger.LogWarning("No events received");
            }
            else
            {
                logger.LogInformation("{Length} event(s) received", results.Length);
            }

            return results;
        }
        catch (OperationCanceledException)
        {
            // The host is winding the run down. Whatever was collected is dropped rather
            // than written, because a part of a run reads like a whole one once stored.
            logger.LogInformation("Collection was cancelled");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to get events");
        }

        return [];
    }
}
