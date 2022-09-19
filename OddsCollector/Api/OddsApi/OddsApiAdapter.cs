namespace OddsCollector.Api.OddsApi;

using Common;
using Models;

/// <summary>
/// Provides access to https://the-odds-api.com/.
/// </summary>
public class OddsApiAdapter : IOddsApiAdapter
{
    private readonly string _apiKey;
    private readonly IClient _apiClient;
    private readonly IOddsApiObjectConverter _converter;

    /// <summary>
    /// A constructor that is suitable for the dependency injection.
    /// </summary>
    /// <param name="config">An <see cref="IConfiguration"/> instance created by the dependency injection container.</param>
    /// <param name="apiClient">An <see cref="IClient"/> instance created by the dependency injection container.</param>
    /// <param name="converter">An <see cref="IOddsApiObjectConverter"/> instance created by the dependency injection container.</param>
    /// <exception cref="ArgumentNullException">Either <paramref name="config"/> or <paramref name="apiClient"/> or <paramref name="converter"/> is null.</exception>
    /// <exception cref="Exception">API key is null or empty.</exception>
    public OddsApiAdapter(IConfiguration config, IClient apiClient, IOddsApiObjectConverter converter)
    {
        ArgumentChecker.NullCheck(config, nameof(config));
        ArgumentChecker.NullCheck(apiClient, nameof(apiClient));
        ArgumentChecker.NullCheck(converter, nameof(converter));

        _apiClient = apiClient;
        _converter = converter;
        _apiKey = ConfigurationReader.GetOddsApiKey(config);

        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new Exception("API key is null or empty.");
        }
    }

    /// <summary>
    /// Gets a list of upcoming football (soccer) events with odds for given list of european leagues.
    /// </summary>
    /// <param name="leagues">A list of european leagues.</param>
    /// <returns>A list of <see cref="SportEvent"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="leagues"/> is null.</exception>
    public async Task<IEnumerable<SportEvent>> GetUpcomingEventsAsync(IEnumerable<string> leagues)
    {
        ArgumentChecker.NullCheck(leagues, nameof(leagues));

        var tasks = leagues.Select(async l => 
            await _apiClient.OddsAsync(
                l, _apiKey, Regions.Eu, Markets.H2h, DateFormat.Iso, OddsFormat.Decimal, null, null));

        var events = await Task.WhenAll(tasks);

        return _converter.ToSportEvents(events);
    }

    /// <summary>
    /// Gets a list of completed football (soccer) events with results for given list of european leagues.
    /// </summary>
    /// <param name="leagues">A list of european leagues.</param>
    /// <returns>A list of completed football (soccer) events.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="leagues"/> is null.</exception>
    /// <remarks>Check ResponseSamples/completed_games.json for the structure.</remarks>
    public async Task<Dictionary<string, string?>> GetCompletedEventsAsync(IEnumerable<string> leagues)
    {
        ArgumentChecker.NullCheck(leagues, nameof(leagues));

        var tasks = 
            leagues.Select(async l => 
                await _apiClient.ScoresAsync(l, _apiKey, 3));

        var events = await Task.WhenAll(tasks);

        return _converter.ToCompletedEvents(events);
    }
}