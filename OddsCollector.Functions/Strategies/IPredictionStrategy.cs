using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Strategies;

internal interface IPredictionStrategy
{
    EventPrediction GetPrediction(UpcomingEvent? upcomingEvent, DateTime? timestamp);
}
