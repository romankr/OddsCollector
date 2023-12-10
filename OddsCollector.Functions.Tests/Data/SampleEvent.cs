using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;
using OddsCollector.Functions.Strategies;

namespace OddsCollector.Functions.Tests.Data;

internal static class SampleEvent
{
    public const string Id = "4acd8f2675ca847ba33eea3664f6c0bb";
    public const string AwayTeam = "Liverpool";
    public const string HomeTeam = "Manchester City";
    public const string Strategy = nameof(AdjustedConsensusStrategy);
    public const string Winner = HomeTeam;

    public const double AwayOdd1 = 4.08;
    public const string Bookmaker1 = "betclic";
    public const double DrawOdd1 = 3.82;
    public const double HomeOdd1 = 1.8;

    public const double AwayOdd2 = 4.33;
    public const string Bookmaker2 = "sport888";
    public const double DrawOdd2 = 4.33;
    public const double HomeOdd2 = 1.7;

    public const double AwayOdd3 = 4.5;
    public const string Bookmaker3 = "mybookieag";
    public const double DrawOdd3 = 4.5;
    public const double HomeOdd3 = 1.67;

    public const bool Completed = true;

    public const string HomeScore = "1";
    public const string AwayScore = "0";

    public static readonly DateTime CommenceTime = new(2023, 11, 25, 12, 30, 0);
    public static readonly Guid TraceId = new("447b57dd-84bc-4e79-95d0-695f7493bf41");
    public static readonly DateTime Timestamp = new(2023, 11, 25, 15, 30, 0);

    public static readonly IEnumerable<Odd> Odds = new List<Odd>
    {
        new OddBuilder().SetSampleData1().Instance,
        new OddBuilder().SetSampleData2().Instance,
        new OddBuilder().SetSampleData3().Instance
    };

    public static readonly Outcome AwayOutcome1 = new() { Name = AwayTeam, Price = AwayOdd1 };

    public static readonly Outcome HomeOutcome1 = new() { Name = HomeTeam, Price = HomeOdd1 };

    public static readonly Outcome DrawOutcome1 = new() { Name = Constants.Draw, Price = DrawOdd1 };

    public static readonly ICollection<Outcome> Outcomes1 =
    [
        HomeOutcome1,
        AwayOutcome1,
        DrawOutcome1
    ];

    private static readonly ICollection<Outcome> Outcomes2 =
    [
        new Outcome { Name = AwayTeam, Price = AwayOdd2 },
        new Outcome { Name = HomeTeam, Price = HomeOdd2 },
        new Outcome { Name = Constants.Draw, Price = DrawOdd2 }
    ];

    private static readonly ICollection<Outcome> Outcomes3 =
    [
        new Outcome { Name = AwayTeam, Price = AwayOdd3 },
        new Outcome { Name = HomeTeam, Price = HomeOdd3 },
        new Outcome { Name = Constants.Draw, Price = DrawOdd3 }
    ];

    public static readonly ICollection<Bookmakers> Bookmakers =
    [
        new Bookmakers
        {
            Key = Bookmaker1,
            Markets =
            [
                new Markets2 { Key = Markets2Key.H2h, Outcomes = Outcomes1 }
            ]
        },
        new Bookmakers
        {
            Key = Bookmaker2,
            Markets =
            [
                new Markets2 { Key = Markets2Key.H2h, Outcomes = Outcomes2 }
            ]
        },
        new Bookmakers
        {
            Key = Bookmaker3,
            Markets =
            [
                new Markets2 { Key = Markets2Key.H2h, Outcomes = Outcomes3 }
            ]
        }
    ];

    public static readonly ScoreModel HomeScoreModel = new() { Name = HomeTeam, Score = HomeScore };

    public static readonly ScoreModel AwayScoreModel = new() { Name = AwayTeam, Score = AwayScore };

    public static readonly ICollection<ScoreModel> Scores =
    [
        HomeScoreModel,
        AwayScoreModel
    ];
}
