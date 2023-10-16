﻿using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using OddsCollector.Common.ExchangeContracts;

namespace OddsCollector.Service.OddsApi.Client;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class OddsClient : IOddsClient
{
    private const DateFormat IsoDateFormat = DateFormat.Iso;
    private const Markets HeadToHeadMarket = Markets.H2h;
    private const Markets2Key HeadToHeadMarketKey = Markets2Key.H2h;
    private const OddsFormat DecimalOddsFormat = OddsFormat.Decimal;
    private const Regions EuropeanRegion = Regions.Eu;
    private const string ApiKeyName = "OddsApi:ApiKey";
    private const string LeaguesKeyName = "OddsApi:Leagues";
    private const int DaysFromToday = 3;
    private readonly string _apiKey;
    private readonly IClient _client;
    private readonly IEnumerable<string> _leagues;

    public OddsClient(IConfiguration? config, IClient? client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));

        if (config is null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        _apiKey = GetApiKey(config);
        _leagues = GetLeagues(config);
    }

    public async Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync()
    {
        return await MakeCall(GetUpcomingEventsAsync, _leagues).ConfigureAwait(false);
    }

    public async Task<IEnumerable<EventResult>> GetEventResultsAsync()
    {
        return await MakeCall(GetEventResultsAsync, _leagues).ConfigureAwait(false);
    }

    private static async Task<IEnumerable<T>> MakeCall<T>(Func<string, Guid, DateTime, Task<IEnumerable<T>>> call,
        IEnumerable<string> leagues)
    {
        var traceId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        var tasks = leagues.Select(async l =>
            await call(l, traceId, timestamp).ConfigureAwait(false));

        var result = await Task.WhenAll(tasks).ConfigureAwait(false);

        return result.SelectMany(e => e);
    }

    private async Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync(string league, Guid traceId,
        DateTime timestamp)
    {
        var events = await _client.OddsAsync(league, _apiKey, EuropeanRegion, HeadToHeadMarket,
            IsoDateFormat, DecimalOddsFormat, null, null).ConfigureAwait(false);

        return ToUpcomingEvents(events, traceId, timestamp);
    }

    private async Task<IEnumerable<EventResult>> GetEventResultsAsync(string league, Guid traceId, DateTime timestamp)
    {
        var results = await _client.ScoresAsync(league, _apiKey, DaysFromToday).ConfigureAwait(false);

        return ToCompletedEvents(results, traceId, timestamp);
    }

    private static string GetApiKey(IConfiguration configuration)
    {
        var key = configuration[ApiKeyName];

        if (string.IsNullOrEmpty(key))
        {
            throw new EmptyApiKeyException($"{ApiKeyName} property is null or empty.");
        }

        return key;
    }

    private static IEnumerable<string> GetLeagues(IConfiguration configuration)
    {
        var leagues = configuration
            .GetSection(LeaguesKeyName)
            .GetChildren()
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrEmpty(v))
            .ToList();

        if (leagues.Count == 0)
        {
            throw new LeaguesNotSpecifiedException($"{LeaguesKeyName} property doesn't have any leagues");
        }

        return leagues!;
    }

    private static IEnumerable<UpcomingEvent> ToUpcomingEvents(ICollection<Anonymous2>? events, Guid traceId,
        DateTime timestamp)
    {
        if (events is null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        return events.Select(e => ToUpcomingEvent(e, traceId, timestamp));
    }

    private static IEnumerable<EventResult> ToCompletedEvents(ICollection<Anonymous3>? events, Guid traceId,
        DateTime timestamp)
    {
        if (events is null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        return events
            .Where(r => r.Completed is not null && r.Completed.Value)
            .Select(r => ToEventResult(r, traceId, timestamp));
    }

    private static UpcomingEvent ToUpcomingEvent(Anonymous2? input, Guid traceId, DateTime timestamp)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        return new UpcomingEvent
        {
            AwayTeam = input.Away_team,
            HomeTeam = input.Home_team,
            Id = input.Id,
            CommenceTime = input.Commence_time,
            Timestamp = timestamp,
            TraceId = traceId,
            Odds = ToOdds(input.Bookmakers, input.Away_team, input.Home_team).ToList()
        };
    }

    private static IEnumerable<Odd> ToOdds(ICollection<Bookmakers>? bookmakers, string? awayTeam, string? homeTeam)
    {
        if (bookmakers is null)
        {
            throw new ArgumentNullException(nameof(bookmakers));
        }

        if (string.IsNullOrEmpty(awayTeam))
        {
            throw new ArgumentOutOfRangeException(nameof(awayTeam), $"{nameof(awayTeam)} is null or empty");
        }

        if (string.IsNullOrEmpty(homeTeam))
        {
            throw new ArgumentOutOfRangeException(nameof(homeTeam), $"{nameof(homeTeam)} is null or empty");
        }

        return bookmakers.Select(b => ToOdd(b, awayTeam, homeTeam));
    }

    private static Odd ToOdd(Bookmakers bookmaker, string awayTeam, string homeTeam)
    {
        if (bookmaker is null)
        {
            throw new ArgumentNullException(nameof(bookmaker));
        }

        var markets = bookmaker.Markets?.First(m => m.Key == HeadToHeadMarketKey);

        return ToOdd(markets, bookmaker.Key, awayTeam, homeTeam);
    }

    private static Odd ToOdd(Markets2? markets, string? bookmaker, string awayTeam, string homeTeam)
    {
        if (markets is null)
        {
            throw new ArgumentNullException(nameof(markets));
        }

        if (string.IsNullOrEmpty(bookmaker))
        {
            throw new ArgumentOutOfRangeException(nameof(bookmaker), $"{nameof(bookmaker)} is null or empty");
        }

        return ToOdd(markets.Outcomes, bookmaker, awayTeam, homeTeam);
    }

    private static Odd ToOdd(ICollection<Outcome>? outcomes, string bookmaker, string awayTeam, string homeTeam)
    {
        if (outcomes is null)
        {
            throw new ArgumentNullException(nameof(outcomes));
        }

        return new Odd
        {
            Bookmaker = bookmaker,
            Home = GetOdd(outcomes, homeTeam),
            Away = GetOdd(outcomes, awayTeam),
            Draw = GetOdd(outcomes, Constants.Draw)
        };
    }

    private static double? GetOdd(IEnumerable<Outcome> outcomes, string oddType)
    {
        return outcomes.First(o => o.Name == oddType).Price;
    }

    private static EventResult ToEventResult(Anonymous3? input, Guid traceId, DateTime timestamp)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

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
        if (scores is null)
        {
            throw new ArgumentNullException(nameof(scores));
        }

        if (string.IsNullOrEmpty(awayTeam))
        {
            throw new ArgumentOutOfRangeException(nameof(awayTeam), $"{nameof(awayTeam)} is null or empty");
        }

        if (string.IsNullOrEmpty(homeTeam))
        {
            throw new ArgumentOutOfRangeException(nameof(homeTeam), $"{nameof(homeTeam)} is null or empty");
        }

        var d = scores.Select(s => ToKeyValuePair(s.Name, s.Score)).ToDictionary(p => p.Key, p => p.Value);

        return GetWinner(d, awayTeam, homeTeam);
    }

    private static KeyValuePair<string, int> ToKeyValuePair(string? name, string? score)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentOutOfRangeException(nameof(name), $"{nameof(name)} is null or empty");
        }

        if (string.IsNullOrEmpty(score))
        {
            throw new ArgumentOutOfRangeException(nameof(score), $"{nameof(score)} is null or empty");
        }

        return new KeyValuePair<string, int>(name, int.Parse(score, CultureInfo.InvariantCulture));
    }

    private static string GetWinner(IReadOnlyDictionary<string, int> scores, string awayTeam, string homeTeam)
    {
        var awayScore = scores[awayTeam];
        var homeScore = scores[homeTeam];

        if (awayScore == homeScore)
        {
            return Constants.Draw;
        }

        return awayScore > homeScore ? awayTeam : homeTeam;
    }
}