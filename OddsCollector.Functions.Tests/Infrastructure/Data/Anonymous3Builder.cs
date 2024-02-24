using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.Tests.Infrastructure.Data;

internal class Anonymous3Builder
{
    public Anonymous3 Instance { get; } = new();

    public Anonymous3Builder SetSampleData()
    {
        return SetId(SampleEvent.Id)
            .SetAwayTeam(SampleEvent.AwayTeam)
            .SetCompleted(SampleEvent.Completed)
            .SetHomeTeam(SampleEvent.HomeTeam)
            .SetCommenceTime(SampleEvent.CommenceTime)
            .SetScores(SampleEvent.Scores);
    }

    public Anonymous3Builder SetAwayTeam(string? awayTeam)
    {
        Instance.Away_team = awayTeam;

        return this;
    }

    public Anonymous3Builder SetCompleted(bool? completed)
    {
        Instance.Completed = completed;

        return this;
    }

    public Anonymous3Builder SetHomeTeam(string? homeTeam)
    {
        Instance.Home_team = homeTeam;

        return this;
    }

    public Anonymous3Builder SetId(string id)
    {
        Instance.Id = id;

        return this;
    }

    public Anonymous3Builder SetCommenceTime(DateTime commenceTime)
    {
        Instance.Commence_time = commenceTime;

        return this;
    }

    public Anonymous3Builder SetScores(ICollection<ScoreModel>? scores)
    {
        Instance.Scores = scores;

        return this;
    }
}
