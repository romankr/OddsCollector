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
    IOriginalUpcomingEventConverter converter) : LeagueClient<UpcomingEvent>(logger, options), IUpcomingEventsClient
{
    private const DateFormat IsoDateFormat = DateFormat.Iso;
    private const Markets HeadToHeadMarket = Markets.H2h;
    private const OddsFormat DecimalOddsFormat = OddsFormat.Decimal;
    private const Regions EuropeanRegion = Regions.Eu;

    protected override string ItemsName => "upcoming events";

    public Task<UpcomingEvent[]> GetUpcomingEventsAsync(CancellationToken cancellationToken)
    {
        return CollectAsync(cancellationToken);
    }

    protected override async Task<IEnumerable<UpcomingEvent>> GetLeagueAsync(string league, string apiKey,
        CancellationToken cancellationToken)
    {
        var events = await client.OddsAsync(league, apiKey, EuropeanRegion, HeadToHeadMarket, IsoDateFormat,
            DecimalOddsFormat, null, null, cancellationToken).ConfigureAwait(false);

        return converter.ToUpcomingEvents(events);
    }
}
