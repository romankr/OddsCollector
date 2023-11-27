using System.Globalization;
using OddsCollector.Common.Models;
using OddsCollector.Common.OddsApi.WebApi;

namespace OddsCollector.Common.OddsApi.Converter;

public class OddsApiObjectConverter : IOddsApiObjectConverter
{
    private const Markets2Key HeadToHeadMarketKey = Markets2Key.H2h;

    public IEnumerable<UpcomingEvent> ToUpcomingEvents(ICollection<Anonymous2>? events, Guid traceId,
        DateTime timestamp)
    {
        ArgumentNullException.ThrowIfNull(events);

        return events.Select(e => ToUpcomingEvent(e, traceId, timestamp));
    }

    public IEnumerable<EventResult> ToEventResults(ICollection<Anonymous3>? events, Guid traceId, DateTime timestamp)
    {
        ArgumentNullException.ThrowIfNull(events);

        return events
            .Where(r => r.Completed != null && r.Completed.Value)
            .Select(r => ToEventResult(r, traceId, timestamp));
    }

    private static UpcomingEvent ToUpcomingEvent(Anonymous2? upcomingEvent, Guid traceId, DateTime timestamp)
    {
        ArgumentNullException.ThrowIfNull(upcomingEvent);

        return new UpcomingEventBuilder()
            .SetAwayTeam(upcomingEvent.Away_team)
            .SetHomeTeam(upcomingEvent.Home_team)
            .SetId(upcomingEvent.Id)
            .SetCommenceTime(upcomingEvent.Commence_time)
            .SetTimestamp(timestamp)
            .SetTraceId(traceId)
            .SetOdds(
                ToOdds(upcomingEvent.Bookmakers, upcomingEvent.Away_team!, upcomingEvent.Home_team!).ToList()
            )
            .Instance;
    }

    private static IEnumerable<Odd> ToOdds(ICollection<Bookmakers>? bookmakers, string awayTeam, string homeTeam)
    {
        ArgumentNullException.ThrowIfNull(bookmakers);

        return bookmakers.Select(b => ToOdd(b, awayTeam, homeTeam));
    }

    private static Odd ToOdd(Bookmakers bookmakers, string awayTeam, string homeTeam)
    {
        ArgumentNullException.ThrowIfNull(bookmakers);

        return ToOdd(bookmakers.Markets, bookmakers.Key, awayTeam, homeTeam);
    }

    private static Odd ToOdd(ICollection<Markets2>? markets, string? bookmaker, string awayTeam, string homeTeam)
    {
        ArgumentNullException.ThrowIfNull(markets);

        if (markets.Count == 0)
        {
            throw new ArgumentException($"{nameof(markets)} cannot be empty", nameof(markets));
        }

        return ToOdd(markets.FirstOrDefault(m => m.Key == HeadToHeadMarketKey), bookmaker, awayTeam, homeTeam);
    }

    private static Odd ToOdd(Markets2? markets, string? bookmaker, string awayTeam, string homeTeam)
    {
        ArgumentNullException.ThrowIfNull(markets);

        return ToOdd(markets.Outcomes, bookmaker, awayTeam, homeTeam);
    }

    private static Odd ToOdd(ICollection<Outcome>? outcomes, string? bookmaker, string awayTeam, string homeTeam)
    {
        ArgumentNullException.ThrowIfNull(outcomes);

        return new OddBuilder()
            .SetBookmaker(bookmaker)
            .SetHome(GetScore(outcomes, homeTeam))
            .SetAway(GetScore(outcomes, awayTeam))
            .SetDraw(GetScore(outcomes, Constants.Draw)).Instance;
    }

    private static double? GetScore(IEnumerable<Outcome> outcomes, string oddType)
    {
        ArgumentNullException.ThrowIfNull(outcomes);

        var matches = outcomes.Where(o => o.Name == oddType).ToList();

        if (matches is null || matches.Count == 0)
        {
            throw new ArgumentException($"{nameof(outcomes)} doesn't have data for {oddType}", nameof(outcomes));
        }

        if (matches.Count > 1)
        {
            throw new ArgumentException($"{nameof(outcomes)} has duplicates for {oddType}", nameof(outcomes));
        }

        return matches.First().Price;
    }

    private static EventResult ToEventResult(Anonymous3 eventResult, Guid traceId, DateTime timestamp)
    {
        return new EventResultBuilder()
            .SetId(eventResult.Id)
            .SetCommenceTime(eventResult.Commence_time)
            .SetTimestamp(timestamp)
            .SetTraceId(traceId)
            .SetWinner(GetWinner(eventResult.Scores, eventResult.Away_team, eventResult.Home_team))
            .Instance;
    }

    private static string GetWinner(ICollection<ScoreModel>? scores, string? awayTeam, string? homeTeam)
    {
        ArgumentNullException.ThrowIfNull(scores);

        if (scores.Count < 2)
        {
            throw new ArgumentException($"{nameof(scores)} must have at least 2 elements", nameof(scores));
        }

        ArgumentException.ThrowIfNullOrEmpty(awayTeam);
        ArgumentException.ThrowIfNullOrEmpty(homeTeam);

        var d = scores.Select(s => ToKeyValuePair(s.Name, s.Score)).ToDictionary(p => p.Key, p => p.Value);

        return GetWinner(d, awayTeam, homeTeam);
    }

    private static KeyValuePair<string, int> ToKeyValuePair(string? name, string? score)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(score);

        return new KeyValuePair<string, int>(name, int.Parse(score, CultureInfo.InvariantCulture));
    }

    private static string GetWinner(Dictionary<string, int> scores, string awayTeam, string homeTeam)
    {
        if (!scores.TryGetValue(awayTeam, out var awayScore))
        {
            throw new ArgumentException($"{nameof(scores)} don't have data for {awayTeam}", nameof(scores));
        }

        if (!scores.TryGetValue(homeTeam, out var homeScore))
        {
            throw new ArgumentException($"{nameof(scores)} don't have data for {homeTeam}", nameof(scores));
        }

        if (awayScore == homeScore)
        {
            return Constants.Draw;
        }

        return awayScore > homeScore ? awayTeam : homeTeam;
    }
}
