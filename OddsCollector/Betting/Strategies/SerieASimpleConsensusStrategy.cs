namespace OddsCollector.Betting.Strategies;

using Common;
using Models;

/// <summary>
/// Strategy that simply calculates consensus outcome from multiple bookmakers for Italian Serie A.
/// </summary>
public class SerieASimpleConsensusStrategy : SimpleConsensusStrategy
{
    /// <summary>
    /// Events that have less than 3 odds and do not belong to Serie A are being discarded.
    /// </summary>
    /// <param name="events">A list of <see cref="SportEvent"/> to filter.</param>
    /// <returns>A filtered list of <see cref="SportEvent"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
    protected override IEnumerable<SportEvent> FilterInitialEvents(IEnumerable<SportEvent> events)
    {
        ArgumentChecker.NullCheck(events, nameof(events));

        return base.FilterInitialEvents(events).Where(e => e.LeagueId == "soccer_italy_serie_a");
    }
}