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
    private readonly ILogger<OddsApiAdapter> _logger;

    /// <summary>
    /// A constructor that is suitable for the dependency injection.
    /// </summary>
    /// <param name="config">An <see cref="IConfiguration"/> instance created by the dependency injection container.</param>
    /// <param name="apiClient">An <see cref="IClient"/> instance created by the dependency injection container.</param>
    /// <param name="logger">An <see cref="ILogger"/> instance created by the dependency injection container.</param>
    /// <exception cref="ArgumentNullException">Either <paramref name="config"/> or <paramref name="apiClient"/> or <paramref name="logger"/> is null.</exception>
    /// <exception cref="Exception">API key is null or empty.</exception>
    public OddsApiAdapter(IConfiguration config, IClient apiClient, ILogger<OddsApiAdapter> logger)
    {
        ArgumentChecker.NullCheck(config, nameof(config));
        ArgumentChecker.NullCheck(apiClient, nameof(apiClient));
        ArgumentChecker.NullCheck(logger, nameof(logger));

        _apiClient = apiClient;
        _logger = logger;
        
        _apiKey = config["OddsApi:ApiKey"];

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

        return events.SelectMany(l => l).Select(ToSportEvent);
    }

    /// <summary>
    /// Converts an <see cref="Anonymous2"/> event into <see cref="SportEvent"/>.
    /// </summary>
    /// <param name="input">An <see cref="Anonymous2"/> event.</param>
    /// <returns>A <see cref="SportEvent"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
    /// <remarks>Check ResponseSamples/events_and_odds.json for the structure.</remarks>
    private static SportEvent ToSportEvent(Anonymous2 input)
    {
        ArgumentChecker.NullCheck(input, nameof(input));

        var result = new SportEvent
        {
            AwayTeam = input.Away_team,
            CommenceTime = input.Commence_time,
            HomeTeam = input.Home_team,
            SportEventId = input.Id,
            LeagueId = input.Sport_key
        };

        result.Odds = ToOdds(input.Bookmakers, result, Markets2Key.H2h).ToList();

        return result;
    }

    /// <summary>
    /// Converts a list of <see cref="Bookmakers"/> into a list of <see cref="Odd"/>.
    /// </summary>
    /// <param name="bookmakers">A list of <see cref="Bookmakers"/>.</param>
    /// <param name="sportEvent">A parent <see cref="Bookmakers"/> instance.</param>
    /// <param name="market">A <see cref="Markets2Key"/> member defining odd format.</param>
    /// <returns>A list of <see cref="Odd"/>.</returns>
    /// <exception cref="ArgumentNullException">Either <paramref name="bookmakers"/> or <paramref name="sportEvent"/> are null.</exception>
    /// <remarks>Check ResponseSamples/events_and_odds.json for the structure.</remarks>
    private static IEnumerable<Odd> ToOdds(ICollection<Bookmakers> bookmakers, SportEvent sportEvent, Markets2Key market)
    {
        ArgumentChecker.NullCheck(bookmakers, nameof(bookmakers));
        ArgumentChecker.NullCheck(sportEvent, nameof(sportEvent));

        foreach (var bookmaker in bookmakers)
        {
            var outcomes = 
                bookmaker.Markets.First(m => m.Key == market).Outcomes;

            if (sportEvent is null || sportEvent.HomeTeam is null || sportEvent.AwayTeam is null)
            {
                continue;
            }

            yield return new Odd
            {
                Bookmaker = bookmaker.Key,
                LastUpdate = bookmaker.Last_update,
                HomeOdd = GetOutcomeValue(outcomes, sportEvent.HomeTeam),
                AwayOdd = GetOutcomeValue(outcomes, sportEvent.AwayTeam),
                DrawOdd = GetOutcomeValue(outcomes, Constants.Draw),
                SportEventId = sportEvent.SportEventId
            };
        }
    }

    /// <summary>
    /// Gets an outcome value for a given outcome type from an outcome list.
    /// </summary>
    /// <param name="outcomes">An outcome list.</param>
    /// <param name="outcomeType">An outcome type.</param>
    /// <returns>An outcome value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="outcomes"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="outcomeType"/>is null or empty.</exception>
    /// <remarks>Check ResponseSamples/events_and_odds.json for the structure.</remarks>
    private static double GetOutcomeValue(ICollection<Outcome> outcomes, string outcomeType)
    {
        ArgumentChecker.NullCheck(outcomes, nameof(outcomes));
        ArgumentChecker.NullOrEmptyCheck(outcomeType, nameof(outcomeType));
            
        return outcomes.First(o => o.Name == outcomeType).Price;
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

        var tasks = leagues.Select(async l => 
            await _apiClient.ScoresAsync(l, _apiKey, 3));

        var events = await Task.WhenAll(tasks);

        var result = new Dictionary<string, string?>();

        foreach (var e in events.SelectMany(l => l))
        {
            // don't sacrifice the whole batch in case of a malformed item
            try
            {
                result.Add(e.Id, ToEventResult(e));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read event result.");
            }
        }

        return result;
    }

    /// <summary>
    /// Converts a completed event <see cref="Anonymous3"/> to a result string with the winning team (or draw).
    /// </summary>
    /// <param name="input">A completed event <see cref="Anonymous3"/>.</param>
    /// <returns>A result string with the winning team (or draw).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
    /// <remarks>Check ResponseSamples/completed_games.json for the structure.</remarks>
    private static string? ToEventResult(Anonymous3 input)
    {
        ArgumentChecker.NullCheck(input, nameof(input));

        if (!input.Completed)
        {
            return null;
        }

        var team1 = input.Scores.ElementAt(0);
        var team2 = input.Scores.ElementAt(1);

        if (team1.Score == team2.Score)
        {
            return Constants.Draw;
        }

        var score1 = int.Parse(team1.Score);
        var score2 = int.Parse(team2.Score);

        return score1 > score2 ? team1.Name : team2.Name;
    }
}