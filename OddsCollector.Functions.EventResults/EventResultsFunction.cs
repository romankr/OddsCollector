using System.Runtime.CompilerServices;
using Microsoft.Azure.Functions.Worker;
using OddsCollector.Common.Models;
using OddsCollector.Common.OddsApi.Client;

[assembly: InternalsVisibleTo("OddsCollector.Functions.EventResults.Tests")]
// DynamicProxyGenAssembly2 is a temporary assembly built by mocking systems that use CastleProxy
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace OddsCollector.Functions.EventResults;

internal sealed class EventResultsFunction(IOddsApiClient? client)
{
    private readonly IOddsApiClient _client = client ?? throw new ArgumentNullException(nameof(client));

    [Function(nameof(EventResultsFunction))]
    [CosmosDBOutput("%CosmosDb:Database%", "%CosmosDb:Container%", Connection = "CosmosDb:Connection")]
    public async Task<EventResult[]> Run([TimerTrigger("%TimerInterval%")] TimerInfo myTimer)
    {
        return (await _client.GetEventResultsAsync(Guid.NewGuid(), DateTime.UtcNow).ConfigureAwait(false)).ToArray();
    }
}
