namespace OddsCollector.Betting;

public class Statistics
{
    /// <summary>
    /// Successful predictions to total ratio.
    /// </summary>
    public double Accuracy { get; set; }

    /// <summary>
    /// Estimated net earnings in score points.
    /// </summary>
    public double EarningsInPoints { get; set; }

    /// <summary>
    /// Estimated net earnings with 10$ bets.
    /// </summary>
    public double Earnings10Bet { get; set; }

    /// <summary>
    /// Estimated net earnings with 20$ bets.
    /// </summary>
    public double Earnings20Bet { get; set; }

    /// <summary>
    /// Estimated net earnings with 50$ bets.
    /// </summary>
    public double Earnings50Bet { get; set; }

    /// <summary>
    /// Total number of completed events.
    /// </summary>
    public int TotalNumberOfEvents { get; set; }

    /// <summary>
    /// Total number of successfully predicted events.
    /// </summary>
    public int NumberOfSuccessfulPredictions { get; set; }
}