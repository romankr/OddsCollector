using System.Text.Json;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;
using OddsCollector.Common.Models;
using OddsCollector.Functions.Notification.CommunicationServices.Configuration;

namespace OddsCollector.Functions.Notification.CommunicationServices;

internal sealed class EmailSender(IOptions<EmailSenderOptions>? options, EmailClient? client) : IEmailSender
{
    private readonly EmailClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly EmailSenderOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    private static readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    public async Task SendEmailAsync(IEnumerable<EventPrediction?> predictions, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(predictions);

        var content = JsonSerializer.Serialize(predictions, _serializerOptions);

        await _client.SendAsync(
            WaitUntil.Completed, _options.SenderAddress, _options.RecipientAddress, _options.Subject,
            content, cancellationToken: cancellationToken).ConfigureAwait(false);

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
