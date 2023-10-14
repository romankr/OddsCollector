namespace OddsCollector.OddsApiService.Client;

using OddsCollector.Common.ExchangeContracts;

internal class OddsClient : IOddsClient
{
    private readonly IClient _client;
    private readonly IConverter _converter;
    private readonly IDefaultParameters _defaults;
    private readonly string _apiKey;
    private readonly IEnumerable<string> _leagues;

    public OddsClient(IConfiguration? config, IClient? client, IConverter? converter, IDefaultParameters? defaults)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));

        if (config is null) throw new ArgumentNullException(nameof(config));
        _apiKey = GetApiKey(config);
        _leagues = GetLeagues(config);
    }

    public async Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync()
    {
        var tasks = _leagues.Select(async l => await _client.OddsAsync(
            l,
            _apiKey,
            _defaults.GetRegions(),
            _defaults.GetMarkets(),
            _defaults.GetDateFormat(),
            _defaults.GetOddsFormat(),
            null,
            null));

        var events = await Task.WhenAll(tasks);

        return _converter.ToUpcomingEvents(events, Guid.NewGuid(), DateTime.UtcNow);
    }

    public async Task<IEnumerable<EventResult>> GetEventResultsAsync()
    {
        var tasks = _leagues.Select(async l => await _client.ScoresAsync(l, _apiKey, _defaults.GetDaysFrom()));

        var events = await Task.WhenAll(tasks);

        return _converter.ToCompletedEvents(events, Guid.NewGuid(), DateTime.UtcNow);
    }

    private static string GetApiKey(IConfiguration configuration)
    {
        if (configuration is null) throw new ArgumentNullException(nameof(configuration));

        var key = configuration["OddsApi:ApiKey"];

        if (string.IsNullOrEmpty(key))
        {
            throw new EmptyApiKeyException("Api Key is null or empty.");
        }

        return key;
    }

    public static IEnumerable<string> GetLeagues(IConfiguration configuration)
    {
        var leagues = configuration
            .GetSection("OddsApi:Leagues")
            .GetChildren()
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrEmpty(v))
            .ToList();

        if (leagues.Count == 0)
        {
            throw new LeaguesNotSpecifiedException("Leagues are not specified");
        }

        return leagues!;
    }
}