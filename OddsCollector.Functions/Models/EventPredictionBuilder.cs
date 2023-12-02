namespace OddsCollector.Functions.Models;

internal class EventPredictionBuilder
{
    public EventPrediction Instance { get; } = new();

    public EventPredictionBuilder SetAwayTeam(string? awayTeam)
    {
        ArgumentException.ThrowIfNullOrEmpty(awayTeam);

        Instance.AwayTeam = awayTeam;

        return this;
    }

    public EventPredictionBuilder SetBookmaker(string? bookmaker)
    {
        ArgumentException.ThrowIfNullOrEmpty(bookmaker);

        Instance.Bookmaker = bookmaker;

        return this;
    }

    public EventPredictionBuilder SetCommenceTime(DateTime? commenceTime)
    {
        ArgumentNullException.ThrowIfNull(commenceTime);

        Instance.CommenceTime = commenceTime.Value;

        return this;
    }

    public EventPredictionBuilder SetHomeTeam(string? homeTeam)
    {
        ArgumentException.ThrowIfNullOrEmpty(homeTeam);

        Instance.HomeTeam = homeTeam;

        return this;
    }

    public EventPredictionBuilder SetId(string? id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        Instance.Id = id;

        return this;
    }

    public EventPredictionBuilder SetStrategy(string? strategy)
    {
        ArgumentException.ThrowIfNullOrEmpty(strategy);

        Instance.Strategy = strategy;

        return this;
    }

    public EventPredictionBuilder SetTimestamp(DateTime? timestamp)
    {
        ArgumentNullException.ThrowIfNull(timestamp);

        Instance.Timestamp = timestamp.Value;

        return this;
    }

    public EventPredictionBuilder SetTraceId(Guid? traceId)
    {
        ArgumentNullException.ThrowIfNull(traceId);

        Instance.TraceId = traceId.Value;

        return this;
    }

    public EventPredictionBuilder SetWinner(string? winner)
    {
        ArgumentException.ThrowIfNullOrEmpty(winner);

        Instance.Winner = winner;

        return this;
    }
}
