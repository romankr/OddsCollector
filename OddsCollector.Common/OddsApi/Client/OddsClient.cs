using OddsCollector.Common.KeyVault.Client;
using OddsCollector.Common.Models;
using OddsCollector.Common.OddsApi.Converter;
using OddsCollector.Common.OddsApi.WebApi;

namespace OddsCollector.Common.OddsApi.Client;

public class OddsClient : IOddsClient
{
    private const DateFormat IsoDateFormat = DateFormat.Iso;
    private const Markets HeadToHeadMarket = Markets.H2h;
    private const OddsFormat DecimalOddsFormat = OddsFormat.Decimal;
    private const Regions EuropeanRegion = Regions.Eu;
    private const int DaysFromToday = 3;
    private readonly IClient _client;
    private readonly IOddsApiObjectConverter _converter;
    private readonly IKeyVaultClient _keyVault;

    public OddsClient(IClient? client, IKeyVaultClient? keyVault, IOddsApiObjectConverter? converter)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _keyVault = keyVault ?? throw new ArgumentNullException(nameof(keyVault));
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
    }

    public async Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync(IEnumerable<string>? leagues)
    {
        if (leagues is null)
        {
            throw new ArgumentNullException(nameof(leagues));
        }

        var result = new List<IEnumerable<UpcomingEvent>>();

        foreach (var league in leagues)
        {
            result.Add(await GetUpcomingEventsAsync(league).ConfigureAwait(false));
        }

        return result.SelectMany(l => l);
    }

    public async Task<IEnumerable<EventResult>> GetEventResultsAsync(IEnumerable<string>? leagues)
    {
        if (leagues is null)
        {
            throw new ArgumentNullException(nameof(leagues));
        }

        var result = new List<IEnumerable<EventResult>>();

        foreach (var league in leagues)
        {
            result.Add(await GetEventResultsAsync(league).ConfigureAwait(false));
        }

        return result.SelectMany(l => l);
    }

    public async Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync(string league)
    {
        var events = await _client.OddsAsync(league, await _keyVault.GetOddsApiKey().ConfigureAwait(false),
            EuropeanRegion, HeadToHeadMarket, IsoDateFormat, DecimalOddsFormat, null, null).ConfigureAwait(false);

        var traceId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        return _converter.ToUpcomingEvents(events, traceId, timestamp);
    }

    public async Task<IEnumerable<EventResult>> GetEventResultsAsync(string league)
    {
        var results = await _client
            .ScoresAsync(league, await _keyVault.GetOddsApiKey().ConfigureAwait(false), DaysFromToday)
            .ConfigureAwait(false);

        var traceId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        return _converter.ToEventResults(results, traceId, timestamp);
    }
}
