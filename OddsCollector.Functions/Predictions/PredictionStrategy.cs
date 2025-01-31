using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

/// <remarks>
///     Based on this article
///     https://www.researchgate.net/publication/320296375_Beating_the_bookies_with_their_own_numbers_-_and_how_the_online_sports_betting_market_is_rigged.
/// </remarks>
internal sealed class PredictionStrategy(IWinnerFinder finder) : IPredictionStrategy
{
    public EventPrediction GetPrediction(UpcomingEvent? upcomingEvent)
    {
        ArgumentNullException.ThrowIfNull(upcomingEvent);

        var winner = finder.GetWinner(upcomingEvent.Odds.ToList());

        return ToEventPrediction(winner, upcomingEvent);
    }

    private static EventPrediction ToEventPrediction(string winner, UpcomingEvent upcomingEvent)
    {
        return new EventPredictionBuilder()
            .SetAwayTeam(upcomingEvent.AwayTeam)
            .SetHomeTeam(upcomingEvent.HomeTeam)
            .SetCommenceTime(upcomingEvent.CommenceTime)
            .SetId(upcomingEvent.Id)
            .SetWinner(winner)
            .Instance;
    }
}
