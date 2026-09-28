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
        TimerInfo timer,
        CancellationToken cancellationToken)
    {
        // A late run still collects everything, so it is worth a warning rather than a skip.
        if (timer.IsPastDue)
        {
            logger.LogWarning("Run is past due");
        }

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
        // Only a cancelled run is caught. Anything else fails the invocation, so a broken
        // run shows up in the failure metrics instead of passing as one that found nothing.
        // The clients already absorb a single league failing, so what arrives here is the run.
        catch (OperationCanceledException exception)
        {
            logger.LogInformation(exception, "Collection was cancelled");
        }

        return [];
    }
}
