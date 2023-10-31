namespace OddsCollector.Service.Notification.ServiceBus;

internal interface IPredictionsProcessor
{
    Task StartProcessingAsync(CancellationToken token);
}
