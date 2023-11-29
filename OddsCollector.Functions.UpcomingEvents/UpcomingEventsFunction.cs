using System.Runtime.CompilerServices;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Common.Models;
using OddsCollector.Common.OddsApi.Client;

[assembly: InternalsVisibleTo("OddsCollector.Functions.UpcomingEvents.Tests")]
// DynamicProxyGenAssembly2 is a temporary assembly built by mocking systems that use CastleProxy
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace OddsCollector.Functions.UpcomingEvents;

internal sealed class UpcomingEventsFunction(ILogger<UpcomingEventsFunction>? logger, IOddsApiClient? client)
{
    private readonly IOddsApiClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly ILogger<UpcomingEventsFunction> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [Function(nameof(UpcomingEventsFunction))]
    [ServiceBusOutput("%ServiceBus:Queue%", Connection = "ServiceBus:Connection")]
    public async Task<UpcomingEvent[]> Run([TimerTrigger("%TimerInterval%")] CancellationToken cancellationToken)
    {
        try
        {
            return (await _client.GetUpcomingEventsAsync(Guid.NewGuid(), DateTime.UtcNow, cancellationToken)
                .ConfigureAwait(false)).ToArray();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to get upcoming events");
        }

        return [];
    }
}
