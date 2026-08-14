using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

/// <remarks>
///     Based on this article
///     https://www.researchgate.net/publication/320296375_Beating_the_bookies_with_their_own_numbers_-_and_how_the_online_sports_betting_market_is_rigged.
/// </remarks>
internal sealed class PredictionStrategy(IOutcomeFinder finder) : IPredictionStrategy
{
    public EventPrediction GetPrediction(UpcomingEvent? upcomingEvent)
    {
        ArgumentNullException.ThrowIfNull(upcomingEvent);

        var outcome = finder.GetOutcome(upcomingEvent.Odds.ToList());

        return ToEventPrediction(outcome, upcomingEvent);
    }

    private static EventPrediction ToEventPrediction(string outcome, UpcomingEvent upcomingEvent)
    {
        return new EventPredictionBuilder()
            .SetAwayTeam(upcomingEvent.AwayTeam)
            .SetHomeTeam(upcomingEvent.HomeTeam)
            .SetCommenceTime(upcomingEvent.CommenceTime)
            .SetId(upcomingEvent.Id)
            .SetOutcome(outcome)
            .Instance;
    }
}
