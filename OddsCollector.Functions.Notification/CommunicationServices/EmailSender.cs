using System.Text.Json;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;
using OddsCollector.Common.Models;
using OddsCollector.Functions.Notification.CommunicationServices.Configuration;

namespace OddsCollector.Functions.Notification.CommunicationServices;

internal sealed class EmailSender : IEmailSender
{
    private readonly EmailSenderOptions _options;

    public EmailSender(IOptions<EmailSenderOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendEmailAsync(IEnumerable<EventPrediction?> predictions)
    {
        if (predictions is null)
        {
            throw new ArgumentNullException(nameof(predictions));
        }

        var content = JsonSerializer.Serialize(predictions, new JsonSerializerOptions { WriteIndented = true });

        var client = new EmailClient(_options.Connection);

        await client.SendAsync(WaitUntil.Completed, _options.SenderAddress, _options.RecipientAddress, _options.Subject,
            content).ConfigureAwait(false);

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
