using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Functions;

internal sealed class UpcomingEventsFunction(ILogger<UpcomingEventsFunction> logger, IUpcomingEventsClient client)
{
    [ExponentialBackoffRetry(2, "00:00:30", "00:02:00")]
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
                logger.LogWarning("No events received");
            }

            return events;
        }
        catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation(exception, "Collection was cancelled");
        }

        return [];
    }
}
