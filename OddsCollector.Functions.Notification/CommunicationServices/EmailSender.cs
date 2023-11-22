using System.Text.Json;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;
using OddsCollector.Common.Models;
using OddsCollector.Functions.Notification.CommunicationServices.Configuration;

namespace OddsCollector.Functions.Notification.CommunicationServices;

internal sealed class EmailSender : IEmailSender
{
    private readonly EmailClient _client;
    private readonly EmailSenderOptions _options;

    public EmailSender(IOptions<EmailSenderOptions>? options, EmailClient? client)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task SendEmailAsync(IEnumerable<EventPrediction?> predictions)
    {
        if (predictions is null)
        {
            throw new ArgumentNullException(nameof(predictions));
        }

        var content = JsonSerializer.Serialize(predictions, new JsonSerializerOptions { WriteIndented = true });

        await _client.SendAsync(WaitUntil.Completed, _options.SenderAddress, _options.RecipientAddress,
            _options.Subject,
            content).ConfigureAwait(false);

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
