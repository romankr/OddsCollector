using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Clients;

internal class UpcomingEventsClient(IOptions<OddsApiClientOptions> options, IClient client, IOddsConverter converter) : IUpcomingEventsClient
{
    public async IAsyncEnumerable<UpcomingEvent> GetUpcomingEventsAsync(Guid traceId, DateTime timestamp, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        List<UpcomingEvent> result = [];

        foreach (var league in options.Value.Leagues)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var events = await client.OddsAsync(league, options.Value.ApiKey, Regions.Eu, Markets.H2h, DateFormat.Iso, OddsFormat.Decimal, null, null, cancellationToken)
                .ConfigureAwait(false);

            result.AddRange(converter.ToUpcomingEvents(events, traceId, timestamp));
        }

        return result;
    }
}
