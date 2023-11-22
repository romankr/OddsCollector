namespace OddsCollector.Common.Models;

public class EventPredictionBuilder
{
    public EventPrediction Instance { get; } = new();

    public EventPredictionBuilder SetAwayTeam(string? awayTeam)
    {
        if (string.IsNullOrEmpty(awayTeam))
        {
            throw new ArgumentException($"{nameof(awayTeam)} cannot be null or empty", nameof(awayTeam));
        }

        Instance.AwayTeam = awayTeam;

        return this;
    }

    public EventPredictionBuilder SetBookmaker(string? bookmaker)
    {
        if (string.IsNullOrEmpty(bookmaker))
        {
            throw new ArgumentException($"{nameof(bookmaker)} cannot be null or empty", nameof(bookmaker));
        }

        Instance.Bookmaker = bookmaker;

        return this;
    }

    public EventPredictionBuilder SetCommenceTime(DateTime? commenceTime)
    {
        if (commenceTime is null)
        {
            throw new ArgumentNullException(nameof(commenceTime));
        }

        Instance.CommenceTime = commenceTime.Value;

        return this;
    }

    public EventPredictionBuilder SetHomeTeam(string? homeTeam)
    {
        if (string.IsNullOrEmpty(homeTeam))
        {
            throw new ArgumentException($"{nameof(homeTeam)} cannot be null or empty", nameof(homeTeam));
        }

        Instance.HomeTeam = homeTeam;

        return this;
    }

    public EventPredictionBuilder SetId(string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException($"{nameof(id)} cannot be null or empty", nameof(id));
        }

        Instance.Id = id;

        return this;
    }

    public EventPredictionBuilder SetStrategy(string? strategy)
    {
        if (string.IsNullOrEmpty(strategy))
        {
            throw new ArgumentException($"{nameof(strategy)} cannot be null or empty", nameof(strategy));
        }

        Instance.Strategy = strategy;

        return this;
    }

    public EventPredictionBuilder SetTimestamp(DateTime? timestamp)
    {
        if (timestamp is null)
        {
            throw new ArgumentNullException(nameof(timestamp));
        }

        Instance.Timestamp = timestamp.Value;

        return this;
    }

    public EventPredictionBuilder SetTraceId(Guid? traceId)
    {
        if (traceId is null)
        {
            throw new ArgumentNullException(nameof(traceId));
        }

        Instance.TraceId = traceId.Value;

        return this;
    }

    public EventPredictionBuilder SetWinner(string? winner)
    {
        if (string.IsNullOrEmpty(winner))
        {
            throw new ArgumentException($"{nameof(winner)} cannot be null or empty", nameof(winner));
        }

        Instance.Winner = winner;

        return this;
    }
}
