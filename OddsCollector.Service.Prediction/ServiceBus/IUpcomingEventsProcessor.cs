namespace OddsCollector.Service.Prediction.ServiceBus;

internal interface IUpcomingEventsProcessor
{
    Task StartProcessingAsync(CancellationToken token);
}
