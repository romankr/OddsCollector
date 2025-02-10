using System.Text;
using System.Text.Json;
using Azure.Core.Amqp;
using Azure.Messaging.ServiceBus;

namespace OddsCollector.Functions.Tests.Infrastructure.ServiceBus;

internal static class ServiceBusReceivedMessageFactory
{
    public static ServiceBusReceivedMessage CreateFromObject(object obj, string? messageId = null)
    {
        var serialized = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(obj))
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
