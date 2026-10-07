using System.Globalization;
using System.Text.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace OddsCollector.Functions.IntegrationTests.Infrastructure.OddsApi;

/// <summary>
///     Stands in for The Odds API, the only dependency of the function app outside Azure.
/// </summary>
/// <param name="league">The league the function app is configured to collect.</param>
/// <param name="apiKey">The key the function app is configured to send; requests without it get no response.</param>
internal sealed class OddsApiStub(string league, string apiKey) : IDisposable
{
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

        Respond($"/v4/sports/{league}/odds", body);
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

    private void Respond(string path, object body)
    {
        _server
            .Given(Request.Create().WithPath(path).WithParam("apiKey", apiKey).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(body)));
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
