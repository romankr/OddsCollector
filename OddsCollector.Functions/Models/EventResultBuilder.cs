namespace OddsCollector.Functions.Models;

internal sealed class EventResultBuilder
{
    public EventResult Instance { get; } = new();

    public EventResultBuilder SetId(string? id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        Instance.Id = id;

        return this;
    }

    public EventResultBuilder SetOutcome(string? outcome)
    {
        ArgumentException.ThrowIfNullOrEmpty(outcome);

        Instance.Outcome = outcome;

        return this;
    }

    public EventResultBuilder SetCommenceTime(DateTime? commenceTime)
    {
        if (!commenceTime.HasValue)
        {
            throw new ArgumentNullException(nameof(commenceTime));
        }

        Instance.CommenceTime = commenceTime.Value;

        return this;
    }
}
