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
        List<Exception> failures = [];

        foreach (var league in options.Value.Leagues)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var results = await client.ScoresAsync(league, options.Value.ApiKey, DaysFromToday, cancellationToken)
                    .ConfigureAwait(false);

                result.AddRange(converter.ToEventResults(results));
            }
            // The host winds the run down: what the leagues already answered is kept rather than thrown away.
            catch (Exception exception) when (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation(exception,
                    "Collection was cancelled, keeping {Count} event results collected so far", result.Count);

                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to get event results for {League}", league);
                failures.Add(exception);
            }
        }

        // A run where no league answered must fail, so that an invalid API key or an exhausted
        // quota shows up as a failed invocation rather than a successful run with no data.
        if (failures.Count > 0 && failures.Count == options.Value.Leagues.Count)
        {
            throw new AggregateException("Failed to get event results for every league", failures);
        }

        return [.. result];
    }
}
