using OddsCollector.Common.Models;

namespace OddsCollector.Functions.Predictions.Strategies;

internal interface IPredictionStrategy
{
    EventPrediction GetPrediction(UpcomingEvent? upcomingEvent, DateTime? timestamp);
}
