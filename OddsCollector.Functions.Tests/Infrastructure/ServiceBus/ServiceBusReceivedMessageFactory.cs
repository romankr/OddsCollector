using System.Text;
using System.Text.Json;
using Azure.Core.Amqp;
using Azure.Messaging.ServiceBus;

namespace OddsCollector.Functions.Tests.Infrastructure.ServiceBus;

internal static class ServiceBusReceivedMessageFactory
{
    public static ServiceBusReceivedMessage CreateFromObject(object obj, string? messageId = null)
    {
        return CreateFromText(JsonSerializer.Serialize(obj), messageId);
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
