namespace OddsCollector.Betting.Strategies;

using Common;
using Models;

public class EplSimpleConsensusStrategy : SimpleConsensusStrategy
{
    protected override IEnumerable<SportEvent> FilterInitialEvents(IEnumerable<SportEvent> events)
    {
        if (events is null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        return base.FilterInitialEvents(events).Where(e => e.LeagueId == Constants.EplKey);
    }
}