﻿namespace OddsCollector.Betting.Strategies;

using Models;

/// <summary>
/// Strategy that simply calculates consensus outcome from multiple bookmakers for English Premier League.
/// </summary>
public class EplSimpleConsensusStrategy : SimpleConsensusStrategy
{
    /// <summary>
    /// Events that have less than 3 odds and do not belong to English Premier League are being discarded.
    /// </summary>
    /// <param name="events">A list of <see cref="SportEvent"/> to filter.</param>
    /// <returns>A filtered list of <see cref="SportEvent"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
    protected override IEnumerable<SportEvent> FilterInitialEvents(IEnumerable<SportEvent> events)
    {
        if (events is null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        return base.FilterInitialEvents(events).Where(e => e.LeagueId == "soccer_epl");
    }
}