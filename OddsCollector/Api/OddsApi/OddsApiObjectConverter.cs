namespace OddsCollector.Api.OddsApi;

using Common;
using Models;

/// <summary>
/// Converts objects received from Odds API.
/// </summary>
/// <remarks>
/// Check ResponseSamples/events_and_odds.json and ResponseSamples/completed_games.json for the structure.
/// </remarks>
public class OddsApiObjectConverter : IOddsApiObjectConverter
{
    private readonly ILogger<OddsApiObjectConverter> _logger;

    /// <summary>
    /// A constructor that is suitable for the dependency injection.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger{OddsApiObjectConverter}"/> instance created by the dependency injection container.</param>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public OddsApiObjectConverter(ILogger<OddsApiObjectConverter> logger)
    {
        ArgumentChecker.NullCheck(logger, nameof(logger));

        _logger = logger;
    }

    /// <summary>
    /// Converts events.
    /// </summary>
    /// <param name="events">A list of <see cref="Anonymous2"/> objects.</param>
    /// <returns>A list of <see cref="SportEvent"/> objects.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
    public IEnumerable<SportEvent> ToSportEvents(ICollection<Anonymous2>[]? events)
    {
        ArgumentChecker.NullCheck(events, nameof(events));

        return events!
            .SelectMany(l => l)
            .Select(ToSportEvent);
    }

    /// <summary>
    /// Converts an <see cref="Anonymous2"/> event into <see cref="SportEvent"/>.
    /// </summary>
    /// <param name="input">An <see cref="Anonymous2"/> event.</param>
    /// <returns>A <see cref="SportEvent"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
    public SportEvent ToSportEvent(Anonymous2 input)
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

        result.Odds = ToOdds(input.Bookmakers, result).ToList();

        return result;
    }

    /// <summary>
    /// Converts a list of <see cref="Bookmakers"/> into a list of <see cref="Odd"/>.
    /// </summary>
    /// <param name="bookmakers">A list of <see cref="Bookmakers"/>.</param>
    /// <param name="sportEvent">A parent <see cref="Bookmakers"/> instance.</param>
    /// <returns>A list of <see cref="Odd"/>.</returns>
    /// <exception cref="ArgumentNullException">Either <paramref name="bookmakers"/> or <paramref name="sportEvent"/> are null.</exception>
    public IEnumerable<Odd> ToOdds(ICollection<Bookmakers> bookmakers, SportEvent sportEvent)
    {
        ArgumentChecker.NullCheck(bookmakers, nameof(bookmakers));
        ArgumentChecker.NullCheck(sportEvent, nameof(sportEvent));

        var result = new List<Odd>();

        // don't sacrifice the whole batch in case of a malformed item
        foreach (var bookmaker in bookmakers)
        {
            try
            {
                result.Add(ToOdd(bookmaker, sportEvent));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read bookmaker.");
            }
        }

        return result;
    }

    /// <summary>
    /// Converts odds.
    /// </summary>
    /// <param name="bookmaker">A <see cref="Bookmakers"/> object.</param>
    /// <param name="sportEvent">A <see cref="SportEvent"/> object.</param>
    /// <returns>A <see cref="Odd"/> object.</returns>
    /// <exception cref="ArgumentNullException">Either <paramref name="bookmaker"/> or <paramref name="sportEvent"/> are null.</exception>
    public Odd ToOdd(Bookmakers? bookmaker, SportEvent? sportEvent)
    {
        ArgumentChecker.NullCheck(bookmaker, nameof(bookmaker));
        ArgumentChecker.NullCheck(sportEvent, nameof(sportEvent));

        var outcomes = ToOutcomes(bookmaker!);

        return new Odd
        {
            Bookmaker = bookmaker!.Key,
            LastUpdate = bookmaker.Last_update,
            HomeOdd = outcomes[sportEvent!.HomeTeam!],
            AwayOdd = outcomes[sportEvent.AwayTeam!],
            DrawOdd = outcomes[Constants.Draw],
            SportEventId = sportEvent.SportEventId
        };
    }

    /// <summary>
    /// Converts a list of outcomes.
    /// </summary>
    /// <param name="bookmaker">A <see cref="Bookmakers"/> object.</param>
    /// <returns>A dictionary with outcomes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="bookmaker"/> is null.</exception>
    public Dictionary<string, double> ToOutcomes(Bookmakers bookmaker)
    {
        ArgumentChecker.NullCheck(bookmaker, nameof(bookmaker));

        return bookmaker
            .Markets
            .First(m => m.Key == Markets2Key.H2h)
            .Outcomes
            .ToDictionary(o => o.Name, o => o.Price);
    }

    /// <summary>
    /// Converts completed events.
    /// </summary>
    /// <param name="events">A list of event lists.</param>
    /// <returns>A list fo completed events.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
    public Dictionary<string, string?> ToCompletedEvents(ICollection<Anonymous3>[]? events)
    {
        ArgumentChecker.NullCheck(events, nameof(events));

        return ToCompletedEvents(events!.SelectMany(l => l));
    }

    /// <summary>
    /// Converts completed events.
    /// </summary>
    /// <param name="events">A list fo events.</param>
    /// <returns>A list of completed events.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
    public Dictionary<string, string?> ToCompletedEvents(IEnumerable<Anonymous3>? events)
    {
        ArgumentChecker.NullCheck(events, nameof(events));

        var result = new Dictionary<string, string?>();

        foreach (var e in events!)
        {
            // don't sacrifice the whole batch in case of a malformed item
            try
            {
                var pair = ToEventResult(e);
                result.Add(pair.Key, pair.Value);
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
    /// <returns>A pair of id and result with the winning team (or draw).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
    public KeyValuePair<string, string?> ToEventResult(Anonymous3 input)
    {
        ArgumentChecker.NullCheck(input, nameof(input));

        if (string.IsNullOrEmpty(input.Id))
        {
            throw new Exception("Id cannot be null");
        }

        if (!input.Completed)
        {
            return new KeyValuePair<string, string?>(input.Id, null);
        }

        var team1 = input.Scores.ElementAt(0);
        var team2 = input.Scores.ElementAt(1);

        if (team1.Score == team2.Score)
        {
            return new KeyValuePair<string, string?>(input.Id, Constants.Draw);
        }

        var score1 = int.Parse(team1.Score);
        var score2 = int.Parse(team2.Score);

        return score1 > score2 
            ? new KeyValuePair<string, string?>(input.Id, team1.Name)
            : new KeyValuePair<string, string?>(input.Id, team2.Name);
    }
}