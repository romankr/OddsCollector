namespace OddsCollector.Betting;

/// <summary>
/// A suggestion yielded by a betting strategy.
/// </summary>
public class BettingSuggestion
{
    /// <summary>
    /// Event ID.
    /// </summary>
    public string? SportEventId { get; set; }

    /// <summary>
    /// The event's date and time in GMT.
    /// </summary>
    public DateTime CommenceTime { get; set; }

    /// <summary>
    /// Name of the away team.
    /// </summary>
    public string? AwayTeam { get; set; }

    /// <summary>
    /// Name of the home team.
    /// </summary>
    public string? HomeTeam { get; set; }

    /// <summary>
    /// Name of the bookmaker with best score.
    /// </summary>
    public string? BestBookmaker { get; set; }

    /// <summary>
    /// Average score calculated by a betting strategy.
    /// </summary>
    public double AverageScore { get; set; }

    /// <summary>
    /// Best score.
    /// </summary>
    public double BestScore { get; set; }

    /// <summary>
    /// Betting strategy prediction.
    /// </summary>
    public string? ExpectedOutcome  { get; set; }

    /// <summary>
    /// Real outcome base on historical data.
    /// Can be empty if the event have not been completed yet.
    /// </summary>
    public string? RealOutcome { get; set; }
}