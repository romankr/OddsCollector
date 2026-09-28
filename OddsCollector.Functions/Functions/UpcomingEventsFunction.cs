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
