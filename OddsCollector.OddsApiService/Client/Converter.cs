namespace OddsCollector.OddsApiService.Client;

using OddsCollector.Common.ExchangeContracts;

internal class Converter : IConverter
{
    private readonly IDefaultParameters _defaults;

    public Converter(IDefaultParameters? defaults)
    {
        _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
    }

    public IEnumerable<UpcomingEvent> ToUpcomingEvents(ICollection<Anonymous2>[]? events, Guid? traceId, DateTime? timestamp)
    {
        if (events is null) throw new ArgumentNullException(nameof(events));
        if (traceId is null) throw new ArgumentNullException(nameof(traceId));
        if (timestamp is null) throw new ArgumentNullException(nameof(timestamp));

        return events.SelectMany(l => l).Select(e => ToUpcomingEvent(e, traceId.Value, timestamp.Value));
    }

    private UpcomingEvent ToUpcomingEvent(Anonymous2? input, Guid traceId, DateTime timestamp)
    {
        if (input is null) throw new ArgumentNullException(nameof(input));

        return new UpcomingEvent
        {
            AwayTeam = input.Away_team,
            HomeTeam = input.Home_team,
            Id = input.Id,
            CommenceTime = input.Commence_time,
            Timestamp = timestamp,
            TraceId = traceId,
            Odds = ToOdds(input.Bookmakers, input.Away_team, input.Home_team)?.ToList()
        };
    }

    private IEnumerable<Odd> ToOdds(ICollection<Bookmakers>? bookmakers, string? awayTeam, string? homeTeam)
    {
        if (bookmakers is null) throw new ArgumentNullException(nameof(bookmakers));
        if (string.IsNullOrEmpty(awayTeam)) throw new ArgumentOutOfRangeException(nameof(awayTeam), $"{nameof(awayTeam)} is null or empty");
        if (string.IsNullOrEmpty(homeTeam)) throw new ArgumentOutOfRangeException(nameof(homeTeam), $"{nameof(homeTeam)} is null or empty");

        return bookmakers.Select(b => ToOdd(b, awayTeam, homeTeam));
    }

    private Odd ToOdd(Bookmakers bookmaker, string awayTeam, string homeTeam)
    {
        if (bookmaker is null) throw new ArgumentNullException(nameof(bookmaker));

        var markets = bookmaker.Markets?.First(m => m.Key == _defaults.GetMarkets2Key());

        return ToOdd(markets, bookmaker.Key, awayTeam, homeTeam);
    }

    private static Odd ToOdd(Markets2? markets, string? bookmaker, string awayTeam, string homeTeam)
    {
        if (markets is null) throw new ArgumentNullException(nameof(markets));
        if (string.IsNullOrEmpty(bookmaker)) throw new ArgumentOutOfRangeException(nameof(bookmaker), $"{nameof(bookmaker)} is null or empty");

        return ToOdd(markets.Outcomes, bookmaker, awayTeam, homeTeam);
    }

    private static Odd ToOdd(ICollection<Outcome>? outcomes, string bookmaker, string awayTeam, string homeTeam)
    {
        if (outcomes is null) throw new ArgumentNullException(nameof(outcomes));

        return new Odd
        {
            Bookmaker = bookmaker,
            Home = GetOdd(outcomes, homeTeam),
            Away = GetOdd(outcomes, awayTeam),
            Draw = GetOdd(outcomes, Constants.Draw)
        };
    }

    private static double? GetOdd(ICollection<Outcome> outcomes, string oddType)
    {
        return outcomes.First(o => o.Name == oddType).Price;
    }

    public IEnumerable<EventResult> ToCompletedEvents(ICollection<Anonymous3>[]? events, Guid? traceId, DateTime? timestamp)
    {
        if (events is null) throw new ArgumentNullException(nameof(events));
        if (traceId is null) throw new ArgumentNullException(nameof(traceId));
        if (timestamp is null) throw new ArgumentNullException(nameof(timestamp));

        return events
                .SelectMany(l => l)
                .Where(r => r is not null && r.Completed is not null && r.Completed.Value)
                .Select(r => ToEventResult(r, traceId.Value, timestamp.Value));
    }

    private static EventResult ToEventResult(Anonymous3? input, Guid traceId, DateTime timestamp)
    {
        if (input is null) throw new ArgumentNullException(nameof(input));

        return new EventResult
        {
            Id = input.Id,
            CommenceTime = input.Commence_time,
            Timestamp = timestamp,
            TraceId = traceId,
            Winner = GetWinner(input.Scores, input.Away_team, input.Home_team)
        };
    }

    private static string GetWinner(ICollection<ScoreModel>? scores, string? awayTeam, string? homeTeam)
    {
        if (scores is null) throw new ArgumentNullException(nameof(scores));

        if (string.IsNullOrEmpty(awayTeam)) throw new ArgumentOutOfRangeException(nameof(awayTeam), $"{nameof(awayTeam)} is null or empty");
        if (string.IsNullOrEmpty(homeTeam)) throw new ArgumentOutOfRangeException(nameof(homeTeam), $"{nameof(homeTeam)} is null or empty");

        var d = scores.Select(s => ToKeyValuePair(s.Name, s.Score)).ToDictionary(p => p.Key, p => p.Value);

        return GetWinner(d, awayTeam, homeTeam);
    }

    private static KeyValuePair<string, int> ToKeyValuePair(string? name, string? score)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentOutOfRangeException(nameof(name), $"{nameof(name)} is null or empty");
        if (string.IsNullOrEmpty(score)) throw new ArgumentOutOfRangeException(nameof(score), $"{nameof(score)} is null or empty");

        return new KeyValuePair<string, int>(name, int.Parse(score));
    }

    private static string GetWinner(Dictionary<string, int> scores, string awayTeam, string homeTeam)
    {
        var awayScore = scores[awayTeam];
        var homeScore = scores[homeTeam];

        if (awayScore == homeScore)
        {
            return Constants.Draw;
        }

        if (awayScore > homeScore)
        {
            return awayTeam;
        }

        return homeTeam;
    }
}