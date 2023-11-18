using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using OddsCollector.Common.Models;
using OddsCollector.Common.OddsApi.Client;
using OddsCollector.Common.OddsApi.Configuration;

namespace OddsCollector.Functions.EventResults;

public class EventResultsFunction
{
    private readonly IOddsClient _client;
    private readonly HashSet<string> _leagues;

    public EventResultsFunction(IOptions<OddsApiOptions> options, IOddsClient? client)
    {
        _leagues = options.Value.Leagues;
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    [Function(nameof(EventResultsFunction))]
    [CosmosDBOutput("%CosmosDb:Database%", "%CosmosDb:Container%", Connection = "CosmosDb:Connection")]
    public async Task<EventResult[]> Run([TimerTrigger("%TimerInterval%")] TimerInfo myTimer)
    {
        return (await _client.GetEventResultsAsync(_leagues).ConfigureAwait(false)).ToArray();
    }
}
