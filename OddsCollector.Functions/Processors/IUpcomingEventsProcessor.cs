using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Processors;

internal interface IUpcomingEventsProcessor
{
    Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync(CancellationToken cancellationToken);
}
