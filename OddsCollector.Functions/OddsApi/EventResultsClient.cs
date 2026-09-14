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
    IOriginalCompletedEventConverter converter) : IEventResultsClient
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

            try
            {
                var results = await client.ScoresAsync(league, options.Value.ApiKey, DaysFromToday, cancellationToken)
                    .ConfigureAwait(false);

                result.AddRange(converter.ToEventResults(results));
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to get event results for {League}", league);
            }
        }

        return [.. result];
    }
}
