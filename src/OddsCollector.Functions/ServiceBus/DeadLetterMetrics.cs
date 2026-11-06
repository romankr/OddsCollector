using System.Diagnostics.Metrics;

namespace OddsCollector.Functions.ServiceBus;

internal sealed class DeadLetterMetrics
{
    public const string MeterName = "OddsCollector.Functions.ServiceBus";
    public const string DeadLetteredMessagesInstrumentName = "oddscollector.servicebus.deadlettered";

    public const string ReasonTag = "reason";

    private readonly Counter<long> _deadLetteredMessages;

    public DeadLetterMetrics(IMeterFactory meterFactory)
    {
        ArgumentNullException.ThrowIfNull(meterFactory);

        _deadLetteredMessages = meterFactory.Create(MeterName).CreateCounter<long>(DeadLetteredMessagesInstrumentName,
            unit: "{message}",
            description: "Number of messages taken out of the dead-letter queue of the upcoming events queue");
    }

    public void RecordDeadLetteredMessage(string reason)
    {
        _deadLetteredMessages.Add(1, new KeyValuePair<string, object?>(ReasonTag, reason));
    }
}
