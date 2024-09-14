using System.Text;
using System.Text.Json;
using Azure.Core.Amqp;
using Azure.Messaging.ServiceBus;

namespace OddsCollector.Functions.Tests.Infrastructure.ServiceBus;

internal static class ServiceBusReceivedMessageFactory
{
    public static IEnumerable<ServiceBusReceivedMessage> CreateFromObjects(IEnumerable<object> objects)
    {
        return objects.Select(CreateFromObject);
    }

    private static ServiceBusReceivedMessage CreateFromObject(object obj)
    {
        var serialized = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(obj))
            .Select(x => new ReadOnlyMemory<byte>([x]));

        var ampqMessage = new AmqpMessageBody(serialized);

        var ampqAnnotatedMessage = new AmqpAnnotatedMessage(ampqMessage);

        return ServiceBusReceivedMessage.FromAmqpMessage(
            ampqAnnotatedMessage, new BinaryData(Array.Empty<byte>()));
    }
}
