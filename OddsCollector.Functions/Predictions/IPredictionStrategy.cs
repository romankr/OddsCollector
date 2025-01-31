using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal interface IPredictionStrategy
{
    EventPrediction GetPrediction(UpcomingEvent? upcomingEvent);
}
