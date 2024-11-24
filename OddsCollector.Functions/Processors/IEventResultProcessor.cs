using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Processors;

internal interface IEventResultProcessor
{
    IAsyncEnumerable<EventResult> GetEventResultsAsync(CancellationToken cancellationToken);
}
