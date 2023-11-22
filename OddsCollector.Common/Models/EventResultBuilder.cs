namespace OddsCollector.Common.Models;

public class EventResultBuilder
{
    public EventResult Instance { get; } = new();

    public EventResultBuilder SetId(string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException($"{nameof(id)} cannot be null or empty", nameof(id));
        }

        Instance.Id = id;

        return this;
    }

    public EventResultBuilder SetWinner(string? winner)
    {
        if (string.IsNullOrEmpty(winner))
        {
            throw new ArgumentException($"{nameof(winner)} cannot be null or empty", nameof(winner));
        }

        Instance.Winner = winner;

        return this;
    }

    public EventResultBuilder SetCommenceTime(DateTime? commenceTime)
    {
        if (commenceTime is null)
        {
            throw new ArgumentNullException(nameof(commenceTime));
        }

        Instance.CommenceTime = commenceTime.Value;

        return this;
    }

    public EventResultBuilder SetTimestamp(DateTime? timestamp)
    {
        if (timestamp is null)
        {
            throw new ArgumentNullException(nameof(timestamp));
        }

        Instance.Timestamp = timestamp.Value;

        return this;
    }

    public EventResultBuilder SetTraceId(Guid? traceId)
    {
        if (traceId is null)
        {
            throw new ArgumentNullException(nameof(traceId));
        }

        Instance.TraceId = traceId.Value;

        return this;
    }
}
