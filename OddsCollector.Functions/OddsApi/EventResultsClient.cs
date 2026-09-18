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
            // Breaking here would return the leagues collected so far, and the caller
            // writes what it is given as though the run had finished.
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var results = await client.ScoresAsync(league, options.Value.ApiKey, DaysFromToday, cancellationToken)
                    .ConfigureAwait(false);

                result.AddRange(converter.ToEventResults(results));
            }
            // A cancelled run is not a league that failed, so it is left to propagate.
            // The filter, rather than catching OperationCanceledException, keeps a client
            // timeout surfacing as the failure of one league.
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogError(exception, "Failed to get event results for {League}", league);
            }
        }

        return [.. result];
    }
}
