using OddsCollector.Common.ServiceBus.Models;

namespace OddsCollector.Service.Notification.Email;

internal interface IEmailSender
{
    Task SendEmailAsync(CancellationToken token);

    EventPrediction Add(EventPrediction? prediction);
}
