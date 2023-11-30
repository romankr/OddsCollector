using Microsoft.Extensions.Options;
using OddsCollector.Common.Models;
using OddsCollector.Common.OddsApi.Configuration;
using OddsCollector.Common.OddsApi.Converter;
using OddsCollector.Common.OddsApi.WebApi;

namespace OddsCollector.Common.OddsApi.Client;

public class OddsApiClient(IOptions<OddsApiClientOptions>? options, IClient? webApiClient,
    IOddsApiObjectConverter? objectConverter) : IOddsApiClient
{
    private const DateFormat IsoDateFormat = DateFormat.Iso;
    private const Markets HeadToHeadMarket = Markets.H2h;
    private const OddsFormat DecimalOddsFormat = OddsFormat.Decimal;
    private const Regions EuropeanRegion = Regions.Eu;
    private const int DaysFromToday = 3;
    private readonly IOddsApiObjectConverter _objectConverter = objectConverter ?? throw new ArgumentNullException(nameof(objectConverter));
    private readonly OddsApiClientOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    private readonly IClient _webApiClient = webApiClient ?? throw new ArgumentNullException(nameof(webApiClient));

    public async Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync(Guid traceId, DateTime timestamp, CancellationToken cancellationToken)
    {
        List<UpcomingEvent> result = [];

        foreach (var league in _options.Leagues)
        {
            var events = await _webApiClient.OddsAsync(league, _options.ApiKey,
                EuropeanRegion, HeadToHeadMarket, IsoDateFormat, DecimalOddsFormat, null, null, cancellationToken).ConfigureAwait(false);

            result.AddRange(_objectConverter.ToUpcomingEvents(events, traceId, timestamp));
        }

        return result;
    }

    public async Task<IEnumerable<EventResult>> GetEventResultsAsync(Guid traceId, DateTime timestamp, CancellationToken cancellationToken)
    {
        List<EventResult> result = [];

        foreach (var league in _options.Leagues)
        {
            var results =
                await _webApiClient.ScoresAsync(league, _options.ApiKey, DaysFromToday, cancellationToken).ConfigureAwait(false);

            result.AddRange(_objectConverter.ToEventResults(results, traceId, timestamp));
        }

        return result;
    }
}
