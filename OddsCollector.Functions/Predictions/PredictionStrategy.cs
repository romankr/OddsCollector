using System.Diagnostics.CodeAnalysis;
using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal sealed class PredictionStrategy(IOutcomePredictor predictor) : IPredictionStrategy
{
    public bool TryGetPrediction(UpcomingEvent upcomingEvent, [NotNullWhen(true)] out EventPrediction? prediction)
    {
        ArgumentNullException.ThrowIfNull(upcomingEvent);

        if (!predictor.TryGetOutcome(upcomingEvent.Odds, out var outcome))
        {
            prediction = null;

            return false;
        }

        prediction = ToEventPrediction(outcome, upcomingEvent);

        return true;
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
