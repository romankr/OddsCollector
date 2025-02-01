using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Processors;

internal interface IUpcomingEventsProcessor
{
    Task<UpcomingEvent[]> GetUpcomingEventsAsync(CancellationToken cancellationToken);
}
