using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi;

internal sealed partial class UpcomingEventsClient(
    ILogger<UpcomingEventsClient> logger,
    IOptions<OddsApiClientOptions> options,
    IClient client,
    IOriginalUpcomingEventConverter converter,
    OddsApiMetrics metrics) : IUpcomingEventsClient
{
    private const DateFormat IsoDateFormat = DateFormat.Iso;
    private const Markets HeadToHeadMarket = Markets.H2h;
    private const OddsFormat DecimalOddsFormat = OddsFormat.Decimal;
    private const Regions EuropeanRegion = Regions.Eu;

    public async Task<UpcomingEvent[]> GetUpcomingEventsAsync(CancellationToken cancellationToken)
    {
        List<UpcomingEvent> result = [];
        List<Exception> failures = [];

        foreach (var league in options.Value.Leagues)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var events = await client.OddsAsync(league, options.Value.ApiKey, EuropeanRegion, HeadToHeadMarket,
                    IsoDateFormat, DecimalOddsFormat, null, null, cancellationToken).ConfigureAwait(false);

                result.AddRange(converter.ToUpcomingEvents(events));
            }
            catch (Exception exception) when (cancellationToken.IsCancellationRequested)
            {
                LogCollectionCancelled(logger, exception, result.Count);

                break;
            }
            catch (Exception exception)
            {
                LogLeagueFailed(logger, exception, league);
                metrics.RecordLeagueFailure(OddsApiMetrics.UpcomingEventsOperation, league);
                failures.Add(exception);
            }
        }

        if (failures.Count > 0 && failures.Count == options.Value.Leagues.Count)
        {
            throw new AggregateException("Failed to get upcoming events for every league", failures);
        }

        return [.. result];
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Collection was cancelled, keeping {Count} upcoming events collected so far")]
    private static partial void LogCollectionCancelled(ILogger logger, Exception exception, int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to get upcoming events for {League}")]
    private static partial void LogLeagueFailed(ILogger logger, Exception exception, string league);
}
