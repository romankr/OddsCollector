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
        List<Exception> failures = [];

        foreach (var league in options.Value.Leagues)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var events = await client.OddsAsync(league, options.Value.ApiKey, EuropeanRegion, HeadToHeadMarket,
                    IsoDateFormat, DecimalOddsFormat, null, null, cancellationToken).ConfigureAwait(false);

                result.AddRange(converter.ToUpcomingEvents(events));
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogError(exception, "Failed to get upcoming events for {League}", league);
                failures.Add(exception);
            }
        }

        // A run where no league answered must fail, so that an invalid API key or an exhausted
        // quota shows up as a failed invocation rather than a successful run with no data.
        if (failures.Count > 0 && failures.Count == options.Value.Leagues.Count)
        {
            throw new AggregateException("Failed to get upcoming events for every league", failures);
        }

        return [.. result];
    }
}
