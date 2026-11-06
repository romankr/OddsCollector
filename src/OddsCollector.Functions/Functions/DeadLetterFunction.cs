using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.ServiceBus;

namespace OddsCollector.Functions.Functions;

// Takes every message out of the dead-letter queue as soon as it gets there: the message is logged and counted, and
// the alert on dead-lettered messages fires on that count. The queue's own dead-letter count only shows how many
// messages are waiting, so an alert on it stays fired and misses the next ones. Nothing of value is dropped:
// UpcomingEventsFunction publishes every upcoming event again on its next run.
internal sealed partial class DeadLetterFunction(ILogger<DeadLetterFunction> logger, DeadLetterMetrics metrics)
{
    public const string UnknownReason = "Unknown";
    public const int MaxLoggedBodyLength = 2048;

    [Function(nameof(DeadLetterFunction))]
    public void Run(
        [ServiceBusTrigger("%ServiceBus:Queue%/$deadletterqueue", Connection = "ServiceBus:Connection",
            AutoCompleteMessages = true)]
        ServiceBusReceivedMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var reason = string.IsNullOrWhiteSpace(message.DeadLetterReason) ? UnknownReason : message.DeadLetterReason;

        LogDeadLetteredMessageRemoved(logger, message.MessageId, reason, message.DeadLetterErrorDescription,
            GetLoggedBody(message.Body));

        metrics.RecordDeadLetteredMessage(reason);
    }

    private static string GetLoggedBody(BinaryData body)
    {
        var text = body.ToString();

        return text.Length <= MaxLoggedBodyLength ? text : $"{text[..MaxLoggedBodyLength]}…";
    }

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Removed dead-lettered message {MessageId}: {DeadLetterReason}: {DeadLetterErrorDescription}. " +
                  "Body: {Body}")]
    private static partial void LogDeadLetteredMessageRemoved(ILogger logger, string messageId,
        string deadLetterReason, string? deadLetterErrorDescription, string body);
}
