using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Processors;

namespace OddsCollector.Functions.Functions;

internal sealed class UpcomingEventsFunction(ILogger<UpcomingEventsFunction> logger, IUpcomingEventsProcessor processor)
{
    [Function(nameof(UpcomingEventsFunction))]
    [ServiceBusOutput("%ServiceBus:Queue%", Connection = "ServiceBus:Connection")]
    public async Task<UpcomingEvent[]> Run(
        [TimerTrigger("%UpcomingEventsFunction:TimerInterval%")]
        CancellationToken cancellationToken)
    {
        try
        {
            return await processor.GetUpcomingEventsAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to get events");
        }

        return [];
    }
}
