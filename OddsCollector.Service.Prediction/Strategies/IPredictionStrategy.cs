using OddsCollector.Common.ExchangeContracts;

namespace OddsCollector.Service.Prediction.Strategies;

internal interface IPredictionStrategy
{
    IEnumerable<EventPrediction> GetPredictions(IEnumerable<UpcomingEvent?>? upcomingEvents, DateTime? timestamp);
}
