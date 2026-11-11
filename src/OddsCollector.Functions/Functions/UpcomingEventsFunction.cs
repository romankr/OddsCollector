using Microsoft.Azure.Functions.Worker;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Functions;

internal sealed partial class UpcomingEventsFunction(ILogger<UpcomingEventsFunction> logger, IUpcomingEventsClient client)
{
    [Function(nameof(UpcomingEventsFunction))]
    [ServiceBusOutput("%ServiceBus:Queue%", Connection = "ServiceBus:Connection")]
    public async Task<UpcomingEvent[]> Run(
        [TimerTrigger("%UpcomingEventsFunction:TimerInterval%")]
        TimerInfo _,
        CancellationToken cancellationToken)
    {
        try
        {
            var events = await client.GetUpcomingEventsAsync(cancellationToken);

            if (events.Length == 0 && !cancellationToken.IsCancellationRequested)
            {
                LogNoEventsReceived(logger);
            }

            return events;
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
