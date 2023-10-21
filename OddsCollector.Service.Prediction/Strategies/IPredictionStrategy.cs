using OddsCollector.Common.ServiceBus.Models;

namespace OddsCollector.Service.Prediction.Strategies;

internal interface IPredictionStrategy
{
    EventPrediction GetPrediction(UpcomingEvent? upcomingEvent, DateTime? timestamp);
}
