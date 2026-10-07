namespace OddsCollector.Functions.Models;

internal sealed class EventResultBuilder
{
    public EventResult Instance { get; } = new();

    public EventResultBuilder SetId(string? id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Instance.Id = id;

        return this;
    }

    public EventResultBuilder SetOutcome(string? outcome)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outcome);

        Instance.Outcome = outcome;

        return this;
    }

    public EventResultBuilder SetCommenceTime(DateTime? commenceTime)
    {
        Instance.CommenceTime = UtcDateTime.Require(commenceTime);

        return this;
    }
}
