namespace OddsCollector.Betting.Strategies;

using Common;
using Models;

public class AdjustedConsensusStrategy : SimpleConsensusStrategy
{
    protected override KeyValuePair<string, double> GetConsensusWinner(SportEvent sportEvent)
    {
        if (sportEvent is null)
        {
            throw new ArgumentNullException(nameof(sportEvent));
        }

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

    private static double GetConsensusProbability(IEnumerable<double> odds, double adjustment)
    {
        if (odds is null)
        {
            throw new ArgumentNullException(nameof(odds));
        }

        if (adjustment == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(adjustment), "Adjustment cannot be 0");
        }

        var average = odds.Average();

        return average == 0 ? 0 : (1 / average) - adjustment;
    }
}