using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Functions;

internal sealed class EventResultsFunction(ILogger<EventResultsFunction> logger, IEventResultsClient client)
{
    // A run fails only when every league failed or the output could not be written. The Odds API
    // and the output binding are retried a few times before the next scheduled run. A retry asks
    // The Odds API again, so the count stays low to save credits; HTTP calls are already retried
    // by the standard resilience handler.
    [ExponentialBackoffRetry(2, "00:00:30", "00:02:00")]
    [Function(nameof(EventResultsFunction))]
    [CosmosDBOutput("%CosmosDb:Database%", "%CosmosDb:EventResultsContainer%",
        Connection = "CosmosDb:Connection")]
    public async Task<EventResult[]> Run(
        [TimerTrigger("%EventResultsFunction:TimerInterval%")]
        TimerInfo _,
        CancellationToken cancellationToken)
    {
        try
        {
            var results = await client.GetEventResultsAsync(cancellationToken);

            // A run cancelled before any league answered has nothing to report but the cancellation,
            // which the client logs.
            if (results.Length == 0 && !cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("No events received");
            }

            return results;
        }
        catch (OperationCanceledException exception)
        {
            logger.LogInformation(exception, "Collection was cancelled");
        }

        return [];
    }
}
