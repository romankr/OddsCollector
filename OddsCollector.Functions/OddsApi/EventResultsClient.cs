using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi;

internal sealed class EventResultsClient(
    ILogger<EventResultsClient> logger,
    IOptions<OddsApiClientOptions> options,
    IClient client,
    IOriginalCompletedEventConverter converter) : LeagueClient<EventResult>(logger, options), IEventResultsClient
{
    private const int DaysFromToday = 3;

    protected override string ItemsName => "event results";

    public Task<EventResult[]> GetEventResultsAsync(CancellationToken cancellationToken)
    {
        return CollectAsync(cancellationToken);
    }

    protected override async Task<IEnumerable<EventResult>> GetLeagueAsync(string league, string apiKey,
        CancellationToken cancellationToken)
    {
        var results = await client.ScoresAsync(league, apiKey, DaysFromToday, cancellationToken)
            .ConfigureAwait(false);

        return converter.ToEventResults(results);
    }
}
