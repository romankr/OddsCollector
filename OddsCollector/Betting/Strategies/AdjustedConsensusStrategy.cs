namespace OddsCollector.Betting.Strategies;

using Common;
using Models;

/// <summary>
/// Strategy that implements ideas from
/// "Beating the bookies with their own numbers - and how the online sports betting market is rigged" research papers.
/// </summary>
/// <remarks>https://www.researchgate.net/publication/320296375_Beating_the_bookies_with_their_own_numbers_-_and_how_the_online_sports_betting_market_is_rigged.</remarks>
public class AdjustedConsensusStrategy : SimpleConsensusStrategy
{
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
    /// <remarks>Consensus score is being adjusted for all outcome types.</remarks>
    protected override KeyValuePair<string, double> GetConsensusWinner(SportEvent sportEvent)
    {
        ArgumentChecker.NullCheck(sportEvent, nameof(sportEvent));

        if (sportEvent.Odds is null)
        {
            throw new Exception("Odds cannot be null or empty");
        }

        if (string.IsNullOrEmpty(sportEvent.HomeTeam))
        {
            throw new Exception("HomeTeam cannot be null or empty");
        }

        if (string.IsNullOrEmpty(sportEvent.AwayTeam))
        {
            throw new Exception("AwayTeam cannot be null or empty");
        }

        var dict = new Dictionary<string, double>
        {
            { Constants.Draw, GetConsensusProbability(sportEvent.Odds.Select(o => o.DrawOdd), 0.057)},
            { sportEvent.HomeTeam, GetConsensusProbability(sportEvent.Odds.Select(o => o.HomeOdd), 0.034)},
            { sportEvent.AwayTeam, GetConsensusProbability(sportEvent.Odds.Select(o => o.AwayOdd), 0.037)}
        };

        return dict.MaxBy(p => p.Value);
    }

    /// <summary>
    /// Gets adjusted consensus probability for given list of odds.
    /// </summary>
    /// <param name="odds">A list of <see cref="Odd"/>.</param>
    /// <param name="adjustment">A consensus probability adjustment.</param>
    /// <returns>A consensus score.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="odds"/>is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="adjustment"/> cannot be 0.</exception>
    private static double GetConsensusProbability(IEnumerable<double> odds, double adjustment)
    {
        ArgumentChecker.NullCheck(odds, nameof(odds));

        if (adjustment == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(adjustment), "Adjustment cannot be 0");
        }

        var average = odds.Average();

        return average == 0 ? 0 : (1 / average) - adjustment;
    }
}