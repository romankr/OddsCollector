using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Functions;

internal class EventResultsFunction(ILogger<EventResultsFunction> logger, IOddsApiClient client)
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
            var results =
                (await client.GetEventResultsAsync(Guid.NewGuid(), DateTime.UtcNow, cancellationToken))
                .ToArray();

            if (results.Length == 0)
            {
                logger.LogWarning("No results received");
            }
            else
            {
                logger.LogInformation("{Length} events received", results.Length);
            }

            return results;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to get event results");
        }

        return [];
    }
}
