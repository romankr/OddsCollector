namespace OddsCollector.Functions.Models;

internal sealed class UpcomingEventBuilder
{
    public UpcomingEvent Instance { get; } = new();

    public UpcomingEventBuilder SetAwayTeam(string? awayTeam)
    {
        ArgumentException.ThrowIfNullOrEmpty(awayTeam);

        Instance.AwayTeam = awayTeam;

        return this;
    }

    public UpcomingEventBuilder SetCommenceTime(DateTime? commenceTime)
    {
        ArgumentNullException.ThrowIfNull(commenceTime);

        Instance.CommenceTime = commenceTime.Value;

        return this;
    }

    public UpcomingEventBuilder SetHomeTeam(string? homeTeam)
    {
        ArgumentException.ThrowIfNullOrEmpty(homeTeam);

        Instance.HomeTeam = homeTeam;

        return this;
    }

    public UpcomingEventBuilder SetId(string? id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        Instance.Id = id;

        return this;
    }

    public UpcomingEventBuilder SetOdds(IEnumerable<Odd>? odds)
    {
        ArgumentNullException.ThrowIfNull(odds);

        Instance.Odds = odds;

        return this;
    }
}
