using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Processors;

internal interface IEventResultProcessor
{
    Task<EventResult[]> GetEventResultsAsync(CancellationToken cancellationToken);
}
