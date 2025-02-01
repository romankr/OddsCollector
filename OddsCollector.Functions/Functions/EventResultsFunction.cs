using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Processors;

namespace OddsCollector.Functions.Functions;

internal sealed class EventResultsFunction(ILogger<EventResultsFunction> logger, IEventResultProcessor processor)
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
            return await processor.GetEventResultsAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to get events");
        }

        return [];
    }
}
