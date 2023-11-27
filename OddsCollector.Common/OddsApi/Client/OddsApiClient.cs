﻿using Microsoft.Extensions.Options;
using OddsCollector.Common.KeyVault.Client;
using OddsCollector.Common.Models;
using OddsCollector.Common.OddsApi.Configuration;
using OddsCollector.Common.OddsApi.Converter;
using OddsCollector.Common.OddsApi.WebApi;

namespace OddsCollector.Common.OddsApi.Client;

public class OddsApiClient(IOptions<OddsApiClientOptions>? options, IClient? webApiClient,
    IKeyVaultClient? keyVaultClient, IOddsApiObjectConverter? objectConverter) : IOddsApiClient
{
    private const DateFormat IsoDateFormat = DateFormat.Iso;
    private const Markets HeadToHeadMarket = Markets.H2h;
    private const OddsFormat DecimalOddsFormat = OddsFormat.Decimal;
    private const Regions EuropeanRegion = Regions.Eu;
    private const int DaysFromToday = 3;
    private readonly IKeyVaultClient _keyVaultClient = keyVaultClient ?? throw new ArgumentNullException(nameof(keyVaultClient));
    private readonly IOddsApiObjectConverter _objectConverter = objectConverter ?? throw new ArgumentNullException(nameof(objectConverter));
    private readonly OddsApiClientOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    private readonly IClient _webApiClient = webApiClient ?? throw new ArgumentNullException(nameof(webApiClient));

    public async Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync(Guid traceId, DateTime timestamp)
    {
        List<UpcomingEvent> result = [];

        foreach (var league in _options.Leagues)
        {
            var events = await _webApiClient.OddsAsync(league,
                await _keyVaultClient.GetOddsApiKey().ConfigureAwait(false),
                EuropeanRegion, HeadToHeadMarket, IsoDateFormat, DecimalOddsFormat, null, null).ConfigureAwait(false);

            result.AddRange(_objectConverter.ToUpcomingEvents(events, traceId, timestamp));
        }

        return result;
    }

    public async Task<IEnumerable<EventResult>> GetEventResultsAsync(Guid traceId, DateTime timestamp)
    {
        List<EventResult> result = [];

        foreach (var league in _options.Leagues)
        {
            var results = await _webApiClient
                .ScoresAsync(league, await _keyVaultClient.GetOddsApiKey().ConfigureAwait(false), DaysFromToday)
                .ConfigureAwait(false);

            result.AddRange(_objectConverter.ToEventResults(results, traceId, timestamp));
        }

        return result;
    }
}