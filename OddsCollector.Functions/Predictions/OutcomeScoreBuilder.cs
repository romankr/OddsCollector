namespace OddsCollector.Functions.Predictions;

internal class OutcomeScoreBuilder
{
    public OutcomeScore Instance { get; } = new();

    public OutcomeScoreBuilder SetScore(double score)
    {
        Instance.Score = score;

        return this;
    }

    public OutcomeScoreBuilder SetOutcome(string? outcome)
    {
        ArgumentException.ThrowIfNullOrEmpty(outcome);

        Instance.Outcome = outcome;

        return this;
    }
}
