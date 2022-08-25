namespace OddsCollector.Betting.Strategies;

using Common;
using Models;

/// <summary>
/// Represents strategy of random winner selection.
/// </summary>
public class RandomStrategy : SimpleConsensusStrategy
{
    private readonly Random _random = new ();

    /// <summary>
    /// Gets consensus winner and average score for given event.
    /// </summary>
    /// <param name="sportEvent"><see cref="SportEvent"/> to analyze.</param>
    /// <returns>A <see cref="KeyValuePair"/> of winner and average score.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sportEvent"/> is null.</exception>
    /// <exception cref="Exception">
    /// Odds cannot be null or empty or
    /// HomeTeam cannot be null or empty or
    /// AwayTeam cannot be null or empty.
    /// </exception>
    protected override KeyValuePair<string, double> GetConsensusWinner(SportEvent sportEvent)
    {
        ArgumentChecker.NullCheck(sportEvent, nameof(sportEvent));

        if (sportEvent.Odds is null)
        {
            throw new Exception("Odds cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(sportEvent.HomeTeam))
        {
            throw new Exception("HomeTeam cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(sportEvent.AwayTeam))
        {
            throw new Exception("AwayTeam cannot be null or empty.");
        }

        var index = _random.Next(1, 3);

        var dict = new Dictionary<string, double>
        {
            { Constants.Draw, sportEvent.Odds.Select(o => o.DrawOdd).Average() },
            { sportEvent.HomeTeam, sportEvent.Odds.Select(o => o.HomeOdd).Average() },
            { sportEvent.AwayTeam, sportEvent.Odds.Select(o => o.AwayOdd).Average() }
        };

        return dict.ElementAt(index);
    }
}