namespace OddsCollector.Functions.Models;

public class EventResultBuilder
{
    public EventResult Instance { get; } = new();

    public EventResultBuilder SetId(string? id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        Instance.Id = id;

        return this;
    }

    public EventResultBuilder SetWinner(string? winner)
    {
        ArgumentException.ThrowIfNullOrEmpty(winner);

        Instance.Winner = winner;

        return this;
    }

    public EventResultBuilder SetCommenceTime(DateTime? commenceTime)
    {
        ArgumentNullException.ThrowIfNull(commenceTime);

        Instance.CommenceTime = commenceTime.Value;

        return this;
    }

    public EventResultBuilder SetTimestamp(DateTime? timestamp)
    {
        ArgumentNullException.ThrowIfNull(timestamp);

        Instance.Timestamp = timestamp.Value;

        return this;
    }

    public EventResultBuilder SetTraceId(Guid? traceId)
    {
        ArgumentNullException.ThrowIfNull(traceId);

        Instance.TraceId = traceId.Value;

        return this;
    }
}
