namespace OddsCollector.Functions.Models;

internal sealed class EventPredictionBuilder
{
    public EventPrediction Instance { get; } = new();

    public EventPredictionBuilder SetAwayTeam(string? awayTeam)
    {
        ArgumentException.ThrowIfNullOrEmpty(awayTeam);

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

    public EventPredictionBuilder SetWinner(string? winner)
    {
        ArgumentException.ThrowIfNullOrEmpty(winner);

        Instance.Winner = winner;

        return this;
    }
}
