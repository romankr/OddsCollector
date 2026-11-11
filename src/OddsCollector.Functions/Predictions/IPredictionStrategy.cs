using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal interface IPredictionStrategy
{
    PredictionResult GetPrediction(UpcomingEvent upcomingEvent);
}
