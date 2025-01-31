using Microsoft.Extensions.Options;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.Converter;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi;

internal sealed class OddsApiClient(
    IOptions<OddsApiClientOptions> options,
    IClient client,
    IOddsApiObjectConverter converter) : IOddsApiClient
{
    private const DateFormat IsoDateFormat = DateFormat.Iso;
    private const Markets HeadToHeadMarket = Markets.H2h;
    private const OddsFormat DecimalOddsFormat = OddsFormat.Decimal;
    private const Regions EuropeanRegion = Regions.Eu;
    private const int DaysFromToday = 3;

    public async Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync(CancellationToken cancellationToken)
    {
        List<UpcomingEvent> result = [];

        foreach (var league in options.Value.Leagues)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var events = await client.OddsAsync(league, options.Value.ApiKey,
                    EuropeanRegion, HeadToHeadMarket, IsoDateFormat,
                    DecimalOddsFormat, null, null, cancellationToken)
                .ConfigureAwait(false);

            result.AddRange(converter.ToUpcomingEvents(events));
        }

        return result;
    }

    public async Task<IEnumerable<EventResult>> GetEventResultsAsync(CancellationToken cancellationToken)
    {
        List<EventResult> result = [];

        foreach (var league in options.Value.Leagues)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var results =
                await client.ScoresAsync(league, options.Value.ApiKey, DaysFromToday, cancellationToken)
                    .ConfigureAwait(false);

            result.AddRange(converter.ToEventResults(results));
        }

        return result;
    }
}
