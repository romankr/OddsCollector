using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Functions;

internal sealed class UpcomingEventsFunction(ILogger<UpcomingEventsFunction> logger, IUpcomingEventsClient client)
{
    [Function(nameof(UpcomingEventsFunction))]
    [ServiceBusOutput("%ServiceBus:Queue%", Connection = "ServiceBus:Connection")]
    public async Task<UpcomingEvent[]> Run(
        [TimerTrigger("%UpcomingEventsFunction:TimerInterval%")]
        CancellationToken cancellationToken)
    {
        try
        {
            var events = await client.GetUpcomingEventsAsync(cancellationToken);

            if (events.Length == 0)
            {
                logger.LogWarning("No events received");
            }
            else
            {
                logger.LogInformation("{Length} event(s) received", events.Length);
            }

            return events;
        }
        catch (OperationCanceledException exception)
        {
            logger.LogInformation(exception, "Collection was cancelled");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to get events");
        }

        return [];
    }
}
