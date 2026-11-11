using Microsoft.Azure.Functions.Worker;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Functions;

internal sealed partial class EventResultsFunction(ILogger<EventResultsFunction> logger, IEventResultsClient client)
{
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

            if (results.Length == 0 && !cancellationToken.IsCancellationRequested)
            {
                LogNoEventsReceived(logger);
            }

            return results;
        }
        catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
        {
            LogCollectionCancelled(logger, exception);
        }

        return [];
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "No events received")]
    private static partial void LogNoEventsReceived(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Collection was cancelled")]
    private static partial void LogCollectionCancelled(ILogger logger, Exception exception);
}
