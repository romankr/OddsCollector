using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.Tests.Infrastructure.Data;

internal class Anonymous2Builder
{
    public Anonymous2 Instance { get; } = new();

    public Anonymous2Builder SetSampleData()
    {
        return SetAwayTeam(SampleEvent.AwayTeam)
            .SetCommenceTime(SampleEvent.CommenceTime)
            .SetHomeTeam(SampleEvent.HomeTeam)
            .SetId(SampleEvent.Id)
            .SetBookmakers(SampleEvent.Bookmakers);
    }

    public Anonymous2Builder SetAwayTeam(string? awayTeam)
    {
        Instance.Away_team = awayTeam;

        return this;
    }

    public Anonymous2Builder SetHomeTeam(string? homeTeam)
    {
        Instance.Home_team = homeTeam;

        return this;
    }

    public Anonymous2Builder SetId(string id)
    {
        Instance.Id = id;

        return this;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public Anonymous2Builder SetCommenceTime(DateTime commenceTime)
    {
        Instance.Commence_time = commenceTime;

        return this;
    }

    public Anonymous2Builder SetBookmakers(ICollection<Bookmakers>? bookmakers)
    {
        Instance.Bookmakers = bookmakers;

        return this;
    }
}
