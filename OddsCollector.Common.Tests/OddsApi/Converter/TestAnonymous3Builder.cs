using OddsCollector.Common.OddsApi.WebApi;

namespace OddsCollector.Common.Tests.OddsApi.Converter;

internal sealed class TestAnonymous3Builder
{
    public const string DefaultAwayTeam = "Liverpool";
    public const bool DefaultCompleted = true;
    public const string DefaultHomeTeam = "Manchester City";
    public const string DefaultId = "4acd8f2675ca847ba33eea3664f6c0bb";
    public static readonly DateTime DefaultCommenceTime = new(2023, 11, 25, 12, 30, 0);

    public static readonly ICollection<ScoreModel> DefaultScores = [
        new() { Name = "Manchester City", Score = "1" },
        new() { Name = "Liverpool", Score = "0" }
    ];

    public Anonymous3 Instance { get; } = new();

    public TestAnonymous3Builder SetDefaults()
    {
        return SetId(DefaultId)
            .SetAwayTeam(DefaultAwayTeam)
            .SetCompleted(DefaultCompleted)
            .SetHomeTeam(DefaultHomeTeam)
            .SetCommenceTime(DefaultCommenceTime)
            .SetScores(DefaultScores);
    }

    public TestAnonymous3Builder SetAwayTeam(string? awayTeam)
    {
        Instance.Away_team = awayTeam;

        return this;
    }

    public TestAnonymous3Builder SetCompleted(bool? completed)
    {
        Instance.Completed = completed;

        return this;
    }

    public TestAnonymous3Builder SetHomeTeam(string? homeTeam)
    {
        Instance.Home_team = homeTeam;

        return this;
    }

    public TestAnonymous3Builder SetId(string id)
    {
        Instance.Id = id;

        return this;
    }

    public TestAnonymous3Builder SetCommenceTime(DateTime commenceTime)
    {
        Instance.Commence_time = commenceTime;

        return this;
    }

    public TestAnonymous3Builder SetScores(ICollection<ScoreModel>? scores)
    {
        Instance.Scores = scores;

        return this;
    }
}
