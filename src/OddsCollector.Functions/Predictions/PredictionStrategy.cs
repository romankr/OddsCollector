using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal sealed class PredictionStrategy(IOutcomePredictor predictor) : IPredictionStrategy
{
    public PredictionResult GetPrediction(UpcomingEvent upcomingEvent)
    {
        ArgumentNullException.ThrowIfNull(upcomingEvent);

        return predictor.GetOutcome(upcomingEvent.Odds) switch
        {
            OutcomeType outcome => ToEventPrediction(outcome, upcomingEvent),
            NoUsableOdds noUsableOdds => noUsableOdds
        };
    }

    private static EventPrediction ToEventPrediction(OutcomeType outcome, UpcomingEvent upcomingEvent)
    {
        return new EventPrediction
        {
            AwayTeam = upcomingEvent.AwayTeam,
            HomeTeam = upcomingEvent.HomeTeam,
            CommenceTime = upcomingEvent.CommenceTime,
            Id = upcomingEvent.Id,
            Outcome = outcome
        };
    }
}
