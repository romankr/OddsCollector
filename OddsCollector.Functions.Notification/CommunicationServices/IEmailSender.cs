using OddsCollector.Common.Models;

namespace OddsCollector.Functions.Notification.CommunicationServices;

internal interface IEmailSender
{
    Task SendEmailAsync(IEnumerable<EventPrediction?> predictions);
}
