using Microsoft.Extensions.Options;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi;

internal sealed class UpcomingEventsClient(
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
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var events = await client.OddsAsync(league, options.Value.ApiKey, EuropeanRegion, HeadToHeadMarket, IsoDateFormat, DecimalOddsFormat, null, null, cancellationToken).ConfigureAwait(false);

            result.AddRange(converter.ToUpcomingEvents(events));
        }

        return [.. result];
    }
}
