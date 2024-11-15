using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Processors;

internal interface IEventResultProcessor
{
    Task<IEnumerable<EventResult>> GetEventResultsAsync(CancellationToken cancellationToken);
}
