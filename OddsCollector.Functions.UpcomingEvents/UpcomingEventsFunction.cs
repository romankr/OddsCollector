using System.Runtime.CompilerServices;
using Microsoft.Azure.Functions.Worker;
using OddsCollector.Common.Models;
using OddsCollector.Common.OddsApi.Client;

[assembly: InternalsVisibleTo("OddsCollector.Functions.UpcomingEvents.Tests")]
// DynamicProxyGenAssembly2 is a temporary assembly built by mocking systems that use CastleProxy
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace OddsCollector.Functions.UpcomingEvents;

internal sealed class UpcomingEventsFunction
{
    private readonly IOddsApiClient _client;

    public UpcomingEventsFunction(IOddsApiClient? client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    [Function(nameof(UpcomingEventsFunction))]
    [ServiceBusOutput("%ServiceBus:Queue%", Connection = "ServiceBus:Connection")]
    public async Task<UpcomingEvent[]> Run([TimerTrigger("%TimerInterval%")] TimerInfo myTimer)
    {
        return (await _client.GetUpcomingEventsAsync(Guid.NewGuid(), DateTime.UtcNow).ConfigureAwait(false)).ToArray();
    }
}
