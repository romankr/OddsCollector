using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.CommunicationServices;

internal interface IEmailSender
{
    Task SendEmailAsync(IEnumerable<EventPrediction?> predictions, CancellationToken cancellationToken);
}
