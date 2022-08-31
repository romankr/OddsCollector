namespace OddsCollector.Api.OddsApi;

using Common;
using Models;

public class OddsApiAdapter : IOddsApiAdapter
{
    private readonly string _apiKey;
    private readonly IClient _apiClient;
    private readonly ILogger<OddsApiAdapter> _logger;

    public OddsApiAdapter(IConfiguration config, IClient apiClient, ILogger<OddsApiAdapter> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
        _apiKey = config["OddsApi:ApiKey"];
    }

    public async Task<IEnumerable<SportEvent>> GetUpcomingEventsAsync(IEnumerable<string> leagues)
    {
        if (leagues is null)
        {
            throw new ArgumentNullException(nameof(leagues));
        }

        var tasks = leagues.Select(async l => 
            await _apiClient.OddsAsync(
                l, _apiKey, Regions.Eu, Markets.H2h, DateFormat.Iso, OddsFormat.Decimal, null, null));

        var events = await Task.WhenAll(tasks);

        return events.SelectMany(l => l).Select(ToSportEvent);
    }

    ///<remarks>Check ApiClient/ResponseSamples/events_and_odds.json for the structure.</remarks>
    private static SportEvent ToSportEvent(Anonymous2 input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

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

    ///<remarks>Check ApiClient/ResponseSamples/events_and_odds.json for the structure.</remarks>
    private static IEnumerable<Odd> ToOdds(ICollection<Bookmakers> bookmakers, SportEvent sportEvent, Markets2Key market)
    {
        if (bookmakers is null)
        {
            throw new ArgumentNullException(nameof(bookmakers));
        }

        if (sportEvent is null)
        {
            throw new ArgumentNullException(nameof(sportEvent));
        }

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
                HomeOdd = GetOutcomePrice(outcomes, sportEvent.HomeTeam),
                AwayOdd = GetOutcomePrice(outcomes, sportEvent.AwayTeam),
                DrawOdd = GetOutcomePrice(outcomes, Constants.Draw),
                SportEventId = sportEvent.SportEventId
            };
        }
    }

    ///<remarks>Check ApiClient/ResponseSamples/events_and_odds.json for the structure.</remarks>
    private static double GetOutcomePrice(ICollection<Outcome> outcomes, string outcomeType)
    {
        if (outcomes is null)
        {
            throw new ArgumentNullException(nameof(outcomes));
        }

        if (string.IsNullOrEmpty(outcomeType))
        {
            throw new ArgumentException("outcomeType is null or empty", nameof(outcomeType));
        }
            
        return outcomes.First(o => o.Name == outcomeType).Price;
    }

    public async Task<Dictionary<string, string?>> GetCompletedEventsAsync(IEnumerable<string> leagues)
    {
        if (leagues is null)
        {
            throw new ArgumentNullException(nameof(leagues));
        }

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

    ///<remarks>Check ApiClient/ResponseSamples/completed_games.json for the structure.</remarks>
    private static string? ToEventResult(Anonymous3 input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

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