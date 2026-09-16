namespace OddsCollector.Functions.Models;

internal sealed class EventPredictionBuilder
{
    public EventPrediction Instance { get; } = new();

    public EventPredictionBuilder SetAwayTeam(string? awayTeam)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(awayTeam);

        Instance.AwayTeam = awayTeam;

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
        ArgumentException.ThrowIfNullOrWhiteSpace(homeTeam);

        Instance.HomeTeam = homeTeam;

        return this;
    }

    public EventPredictionBuilder SetId(string? id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Instance.Id = id;

        return this;
    }

    public EventPredictionBuilder SetWinner(string? winner)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(winner);

        Instance.Winner = winner;

        return this;
    }
}
