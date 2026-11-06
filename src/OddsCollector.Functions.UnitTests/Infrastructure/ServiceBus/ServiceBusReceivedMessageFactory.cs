using System.Text;
using System.Text.Json;
using Azure.Core.Amqp;
using Azure.Messaging.ServiceBus;

namespace OddsCollector.Functions.UnitTests.Infrastructure.ServiceBus;

internal static class ServiceBusReceivedMessageFactory
{
    // Service Bus keeps the dead-letter reason and description in these application properties.
    private const string DeadLetterReasonProperty = "DeadLetterReason";
    private const string DeadLetterErrorDescriptionProperty = "DeadLetterErrorDescription";

    public static ServiceBusReceivedMessage CreateFromObject(object obj, string? messageId = null)
    {
        return CreateFromText(JsonSerializer.Serialize(obj), messageId);
    }

    public static ServiceBusReceivedMessage CreateDeadLettered(string text, string messageId, string? reason,
        string? description)
    {
        Dictionary<string, object> properties = [];

        if (reason is not null)
        {
            properties[DeadLetterReasonProperty] = reason;
        }

        if (description is not null)
        {
            properties[DeadLetterErrorDescriptionProperty] = description;
        }

        return ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromString(text),
            messageId: messageId, properties: properties);
    }

    public static ServiceBusReceivedMessage CreateFromText(string text, string? messageId = null)
    {
        var serialized = Encoding.ASCII.GetBytes(text)
            .Select(x => new ReadOnlyMemory<byte>([x]));

        var message = new AmqpMessageBody(serialized);

        var annotatedMessage = new AmqpAnnotatedMessage(message)
        {
            Properties =
            {
                MessageId = messageId is not null
                    ? new AmqpMessageId(messageId)
                    : null
            }
        };

        return ServiceBusReceivedMessage.FromAmqpMessage(
            annotatedMessage, new BinaryData([]));
    }
}
