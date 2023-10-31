using System.Collections.Concurrent;
using Azure;
using Azure.Communication.Email;
using Newtonsoft.Json;
using OddsCollector.Common.Configuration;
using OddsCollector.Common.ServiceBus.Models;

namespace OddsCollector.Service.Notification.Email;

internal sealed class EmailSender : IEmailSender
{
    private readonly string _connectionString;

    private readonly ConcurrentDictionary<string, EventPrediction> _dictionary = new();
    private readonly string _recipientAddress;
    private readonly string _senderAddress;
    private readonly string _subject;

    public EmailSender(IConfiguration configuration)
    {
        _connectionString = configuration.GetRequiredSection<EmailSenderOptions>().ConnectionString;
        _senderAddress = configuration.GetRequiredSection<EmailSenderOptions>().SenderAddress;
        _recipientAddress = configuration.GetRequiredSection<EmailSenderOptions>().RecipientAddress;
        _subject = configuration.GetRequiredSection<EmailSenderOptions>().Subject;
    }

    public EventPrediction Add(EventPrediction? prediction)
    {
        if (prediction is null)
        {
            throw new ArgumentNullException(nameof(prediction));
        }

        return _dictionary.GetOrAdd(prediction.Id!, prediction);
    }

    public async Task SendEmailAsync(CancellationToken token)
    {
        var events = _dictionary.Values.ToList();

        _dictionary.Clear();

        var content = JsonConvert.SerializeObject(events, Formatting.Indented);

        var client = new EmailClient(_connectionString);

        await client.SendAsync(WaitUntil.Completed, _senderAddress, _recipientAddress, _subject, content,
                cancellationToken: token)
            .ConfigureAwait(false);

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
