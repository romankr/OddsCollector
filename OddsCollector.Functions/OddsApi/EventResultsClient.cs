using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi;

internal sealed partial class EventResultsClient(
    ILogger<EventResultsClient> logger,
    IOptions<OddsApiClientOptions> options,
    IClient client,
    IOriginalCompletedEventConverter converter,
    OddsApiMetrics metrics) : IEventResultsClient
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
            catch (Exception exception) when (cancellationToken.IsCancellationRequested)
            {
                LogCollectionCancelled(logger, exception, result.Count);

                break;
            }
            catch (Exception exception)
            {
                LogLeagueFailed(logger, exception, league);
                metrics.RecordLeagueFailure(OddsApiMetrics.EventResultsOperation, league);
                failures.Add(exception);
            }
        }

        if (failures.Count > 0 && failures.Count == options.Value.Leagues.Count)
        {
            throw new AggregateException("Failed to get event results for every league", failures);
        }

        return [.. result];
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Collection was cancelled, keeping {Count} event results collected so far")]
    private static partial void LogCollectionCancelled(ILogger logger, Exception exception, int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to get event results for {League}")]
    private static partial void LogLeagueFailed(ILogger logger, Exception exception, string league);
}
