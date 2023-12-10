using Microsoft.Extensions.Options;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.Converter;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi;

internal class OddsApiClient(
    IOptions<OddsApiClientOptions> options,
    IClient webApiClient,
    IOddsApiObjectConverter objectConverter) : IOddsApiClient
{
    private const DateFormat IsoDateFormat = DateFormat.Iso;
    private const Markets HeadToHeadMarket = Markets.H2h;
    private const OddsFormat DecimalOddsFormat = OddsFormat.Decimal;
    private const Regions EuropeanRegion = Regions.Eu;
    private const int DaysFromToday = 3;

    public async Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync(Guid traceId, DateTime timestamp,
        CancellationToken cancellationToken)
    {
        List<UpcomingEvent> result = [];

        foreach (var league in options.Value.Leagues)
        {
            var events = await webApiClient.OddsAsync(league, options.Value.ApiKey,
                    EuropeanRegion, HeadToHeadMarket, IsoDateFormat, DecimalOddsFormat, null, null, cancellationToken)
                .ConfigureAwait(false);

            result.AddRange(objectConverter.ToUpcomingEvents(events, traceId, timestamp));
        }

        return result;
    }

    public async Task<IEnumerable<EventResult>> GetEventResultsAsync(Guid traceId, DateTime timestamp,
        CancellationToken cancellationToken)
    {
        List<EventResult> result = [];

        foreach (var league in options.Value.Leagues)
        {
            var results =
                await webApiClient.ScoresAsync(league, options.Value.ApiKey, DaysFromToday, cancellationToken)
                    .ConfigureAwait(false);

            result.AddRange(objectConverter.ToEventResults(results, traceId, timestamp));
        }

        return result;
    }
}
