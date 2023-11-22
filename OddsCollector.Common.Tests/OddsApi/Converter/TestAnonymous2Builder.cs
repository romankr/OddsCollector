﻿using OddsCollector.Common.OddsApi.WebApi;

namespace OddsCollector.Common.Tests.OddsApi.Converter;

internal class TestAnonymous2Builder
{
    public const string DefaultAwayTeam = "Liverpool";
    public const string DefaultHomeTeam = "Manchester City";
    public const string DefaultId = "4acd8f2675ca847ba33eea3664f6c0bb";
    public static readonly DateTime DefaultCommenceTime = new(2023, 11, 25, 12, 30, 0);

    public static readonly ICollection<Bookmakers> DefaultBookmakers = new List<Bookmakers>
    {
        new()
        {
            Key = "betclic",
            Markets = new List<Markets2>
            {
                new()
                {
                    Key = Markets2Key.H2h,
                    Outcomes = new List<Outcome>
                    {
                        new() { Name = "Liverpool", Price = 4.08 },
                        new() { Name = "Manchester City", Price = 1.7 },
                        new() { Name = "Draw", Price = 3.82 }
                    }
                }
            }
        },
        new()
        {
            Key = "sport888",
            Markets = new List<Markets2>
            {
                new()
                {
                    Key = Markets2Key.H2h,
                    Outcomes = new List<Outcome>
                    {
                        new() { Name = "Liverpool", Price = 4.33 },
                        new() { Name = "Manchester City", Price = 1.7 },
                        new() { Name = "Draw", Price = 4.33 }
                    }
                }
            }
        }
    };

    public Anonymous2 Instance { get; } = new();

    public TestAnonymous2Builder SetDefaults()
    {
        return SetAwayTeam(DefaultAwayTeam)
            .SetCommenceTime(DefaultCommenceTime)
            .SetHomeTeam(DefaultHomeTeam)
            .SetId(DefaultId)
            .SetBookmakers(DefaultBookmakers);
    }

    public TestAnonymous2Builder SetAwayTeam(string? awayTeam)
    {
        Instance.Away_team = awayTeam;

        return this;
    }

    public TestAnonymous2Builder SetHomeTeam(string? homeTeam)
    {
        Instance.Home_team = homeTeam;

        return this;
    }

    public TestAnonymous2Builder SetId(string id)
    {
        Instance.Id = id;

        return this;
    }

    public TestAnonymous2Builder SetCommenceTime(DateTime commenceTime)
    {
        Instance.Commence_time = commenceTime;

        return this;
    }

    public TestAnonymous2Builder SetBookmakers(ICollection<Bookmakers>? bookmakers)
    {
        Instance.Bookmakers = bookmakers;

        return this;
    }
}
