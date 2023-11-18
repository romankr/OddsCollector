using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using OddsCollector.Common.Models;
using OddsCollector.Common.OddsApi.Client;
using OddsCollector.Common.OddsApi.Configuration;

namespace OddsCollector.Functions.UpcomingEvents;

public class UpcomingEventsFunction
{
    private readonly IOddsClient _client;
    private readonly HashSet<string> _leagues;

    public UpcomingEventsFunction(IOptions<OddsApiOptions> options, IOddsClient? client)
    {
        _leagues = options.Value.Leagues;
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    [Function(nameof(UpcomingEventsFunction))]
    [ServiceBusOutput("%ServiceBus:Queue%", Connection = "ServiceBus:Connection")]
    public async Task<UpcomingEvent[]> Run([TimerTrigger("%TimerInterval%")] TimerInfo myTimer)
    {
        return (await _client.GetUpcomingEventsAsync(_leagues).ConfigureAwait(false)).ToArray();
    }
}
