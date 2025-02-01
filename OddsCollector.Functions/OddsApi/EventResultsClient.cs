using Microsoft.Extensions.Options;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi;

internal sealed class EventResultsClient(IOptions<OddsApiClientOptions> options, IClient client, IOriginalCompletedEventConverter converter) : IEventResultsClient
{
    private const int DaysFromToday = 3;

    public async Task<EventResult[]> GetEventResultsAsync(CancellationToken cancellationToken)
    {
        List<EventResult> result = [];

        foreach (var league in options.Value.Leagues)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var results = await client.ScoresAsync(league, options.Value.ApiKey, DaysFromToday, cancellationToken).ConfigureAwait(false);

            result.AddRange(converter.ToEventResults(results));
        }

        return [.. result];
    }
}
