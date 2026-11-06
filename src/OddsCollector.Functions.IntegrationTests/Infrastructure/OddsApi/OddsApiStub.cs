using System.Globalization;
using System.Net;
using System.Text.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace OddsCollector.Functions.IntegrationTests.Infrastructure.OddsApi;

internal sealed class OddsApiStub(string league, string apiKey) : IDisposable
{
    private readonly Dictionary<string, Guid> _mappings = [];
    private readonly WireMockServer _server = WireMockServer.Start();

    public string BaseUrl => _server.Url!;

    public void Dispose()
    {
        _server.Stop();
        _server.Dispose();
    }

    public void SetUpcomingEvents(IEnumerable<OddsApiEvent> events)
    {
        var body = events.Select(e => new
        {
            id = e.Id,
            sport_key = league,
            sport_title = league,
            commence_time = ToIso(e.CommenceTime),
            home_team = e.HomeTeam,
            away_team = e.AwayTeam,
            bookmakers = e.Bookmakers.Select(b => new
            {
                key = b.Key,
                title = b.Key,
                last_update = ToIso(e.CommenceTime.AddDays(-1)),
                markets = new[]
                {
                    new
                    {
                        key = "h2h",
                        outcomes = new[]
                        {
                            new { name = e.HomeTeam, price = b.Home },
                            new { name = e.AwayTeam, price = b.Away },
                            new { name = "Draw", price = b.Draw }
                        }
                    }
                }
            })
        });

        Respond(UpcomingEventsPath, body);
    }

    public void FailUpcomingEvents(HttpStatusCode statusCode)
    {
        Respond(UpcomingEventsPath, statusCode, body: null);
    }

    public int CountUpcomingEventsRequests()
    {
        return _server.LogEntries.Count(e => e.RequestMessage?.Path == UpcomingEventsPath);
    }

    public void SetCompletedEvents(IEnumerable<OddsApiCompletedEvent> events)
    {
        var body = events.Select(e => new
        {
            id = e.Id,
            sport_key = league,
            sport_title = league,
            commence_time = ToIso(e.CommenceTime),
            completed = true,
            home_team = e.HomeTeam,
            away_team = e.AwayTeam,
            scores = new[]
            {
                new { name = e.HomeTeam, score = e.HomeScore.ToString(CultureInfo.InvariantCulture) },
                new { name = e.AwayTeam, score = e.AwayScore.ToString(CultureInfo.InvariantCulture) }
            },
            last_update = ToIso(e.CommenceTime.AddHours(2))
        });

        Respond($"/v4/sports/{league}/scores", body);
    }

    private string UpcomingEventsPath => $"/v4/sports/{league}/odds";

    private void Respond(string path, object body)
    {
        Respond(path, HttpStatusCode.OK, body);
    }

    private void Respond(string path, HttpStatusCode statusCode, object? body)
    {
        if (!_mappings.TryGetValue(path, out var mapping))
        {
            mapping = Guid.NewGuid();
            _mappings[path] = mapping;
        }

        _server
            .Given(Request.Create().WithPath(path).WithParam("apiKey", apiKey).UsingGet())
            .WithGuid(mapping)
            .RespondWith(Response.Create()
                .WithStatusCode(statusCode)
                .WithHeader("Content-Type", "application/json")
                .WithBody(body is null ? string.Empty : JsonSerializer.Serialize(body)));
    }

    private static string ToIso(DateTime value)
    {
        return value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
    }
}

internal sealed record OddsApiEvent(
    string Id,
    DateTime CommenceTime,
    string HomeTeam,
    string AwayTeam,
    IReadOnlyCollection<OddsApiBookmaker> Bookmakers);

internal sealed record OddsApiBookmaker(string Key, double Home, double Away, double Draw);

internal sealed record OddsApiCompletedEvent(
    string Id,
    DateTime CommenceTime,
    string HomeTeam,
    string AwayTeam,
    int HomeScore,
    int AwayScore);
