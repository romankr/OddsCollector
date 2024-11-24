using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Clients;

internal class EventResultsClient(IOptions<OddsApiClientOptions> options, IClient client, IScoresConverter converter) : IEventResultsClient
{
    public async IAsyncEnumerable<EventResult> GetEventResultsAsync(Guid traceId, DateTime timestamp, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        List<EventResult> result = [];

        foreach (var league in options.Value.Leagues)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var results = await client.ScoresAsync(league, options.Value.ApiKey, 3, cancellationToken).ConfigureAwait(false);

            result.AddRange(converter.ToEventResults(results, traceId, timestamp));
        }

        return result;
    }
}
