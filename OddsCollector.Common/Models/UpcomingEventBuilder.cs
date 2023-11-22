namespace OddsCollector.Common.Models;

public class UpcomingEventBuilder
{
    public UpcomingEvent Instance { get; } = new();

    public UpcomingEventBuilder SetAwayTeam(string? awayTeam)
    {
        if (string.IsNullOrEmpty(awayTeam))
        {
            throw new ArgumentException($"{nameof(awayTeam)} cannot be null or empty", nameof(awayTeam));
        }

        Instance.AwayTeam = awayTeam;

        return this;
    }

    public UpcomingEventBuilder SetCommenceTime(DateTime? commenceTime)
    {
        if (commenceTime is null)
        {
            throw new ArgumentNullException(nameof(commenceTime));
        }

        Instance.CommenceTime = commenceTime.Value;

        return this;
    }

    public UpcomingEventBuilder SetHomeTeam(string? homeTeam)
    {
        if (string.IsNullOrEmpty(homeTeam))
        {
            throw new ArgumentException($"{nameof(homeTeam)} cannot be null or empty", nameof(homeTeam));
        }

        Instance.HomeTeam = homeTeam;

        return this;
    }

    public UpcomingEventBuilder SetId(string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException($"{nameof(id)} cannot be null or empty", nameof(id));
        }

        Instance.Id = id;

        return this;
    }

    public UpcomingEventBuilder SetTimestamp(DateTime? timestamp)
    {
        if (timestamp is null)
        {
            throw new ArgumentNullException(nameof(timestamp));
        }

        Instance.Timestamp = timestamp.Value;

        return this;
    }

    public UpcomingEventBuilder SetTraceId(Guid? traceId)
    {
        if (traceId is null)
        {
            throw new ArgumentNullException(nameof(traceId));
        }

        Instance.TraceId = traceId.Value;

        return this;
    }

    public UpcomingEventBuilder SetOdds(IEnumerable<Odd>? odds)
    {
        if (odds is null)
        {
            throw new ArgumentNullException(nameof(odds));
        }

        Instance.Odds = odds;

        return this;
    }
}
