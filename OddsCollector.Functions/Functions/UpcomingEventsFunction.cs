using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Functions;

internal class UpcomingEventsFunction(ILogger<UpcomingEventsFunction> logger, IOddsApiClient client)
{
    [Function(nameof(UpcomingEventsFunction))]
    [ServiceBusOutput("%ServiceBus:Queue%", Connection = "ServiceBus:Connection")]
    public async Task<UpcomingEvent[]> Run(
        [TimerTrigger("%UpcomingEventsFunction:TimerInterval%")]
        CancellationToken cancellationToken)
    {
        try
        {
            var events =
                (await client.GetUpcomingEventsAsync(Guid.NewGuid(), DateTime.UtcNow, cancellationToken))
                .ToArray();

            if (events.Length == 0)
            {
                logger.LogWarning("No events received");
            }
            else
            {
                logger.LogInformation("{Length} events received", events.Length);
            }

            return events;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to get upcoming events");
        }

        return [];
    }
}
