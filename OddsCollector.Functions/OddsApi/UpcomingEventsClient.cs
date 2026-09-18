using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi;

internal sealed class UpcomingEventsClient(
    ILogger<UpcomingEventsClient> logger,
    IOptions<OddsApiClientOptions> options,
    IClient client,
    IOriginalUpcomingEventConverter converter) : IUpcomingEventsClient
{
    private const DateFormat IsoDateFormat = DateFormat.Iso;
    private const Markets HeadToHeadMarket = Markets.H2h;
    private const OddsFormat DecimalOddsFormat = OddsFormat.Decimal;
    private const Regions EuropeanRegion = Regions.Eu;

    public async Task<UpcomingEvent[]> GetUpcomingEventsAsync(CancellationToken cancellationToken)
    {
        List<UpcomingEvent> result = [];

        foreach (var league in options.Value.Leagues)
        {
            // Breaking here would return the leagues collected so far, and the caller
            // writes what it is given as though the run had finished.
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var events = await client.OddsAsync(league, options.Value.ApiKey, EuropeanRegion, HeadToHeadMarket,
                    IsoDateFormat, DecimalOddsFormat, null, null, cancellationToken).ConfigureAwait(false);

                result.AddRange(converter.ToUpcomingEvents(events));
            }
            // One unavailable or malformed league must not discard the ones that worked. A
            // cancelled run is not such a league, so it is left to propagate; the filter,
            // rather than catching OperationCanceledException, keeps a client timeout
            // surfacing as the failure of one league.
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogError(exception, "Failed to get upcoming events for {League}", league);
            }
        }

        return [.. result];
    }
}
