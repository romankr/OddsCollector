using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Processors;

internal interface IUpcomingEventsProcessor
{
    IAsyncEnumerable<UpcomingEvent> GetUpcomingEventsAsync(CancellationToken cancellationToken);
}
