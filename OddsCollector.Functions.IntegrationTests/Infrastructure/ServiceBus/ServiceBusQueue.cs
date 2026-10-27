using Azure.Messaging.ServiceBus;

namespace OddsCollector.Functions.IntegrationTests.Infrastructure.ServiceBus;

internal sealed class ServiceBusQueue(string connectionString, string queueName) : IAsyncDisposable
{
    private const int MaxPeekedMessages = 100;

    private readonly ServiceBusClient _client = new(connectionString);

    public ValueTask DisposeAsync()
    {
        return _client.DisposeAsync();
    }

    public async Task SendAsync(string messageId, string body, CancellationToken cancellationToken)
    {
        await using var sender = _client.CreateSender(queueName);

        await sender.SendMessageAsync(
            new ServiceBusMessage(body) { MessageId = messageId, ContentType = "application/json" },
            cancellationToken);
    }

    public Task<ServiceBusReceivedMessage?> TryPeekAsync(string messageId, CancellationToken cancellationToken)
    {
        return TryPeekAsync(SubQueue.None, messageId, cancellationToken);
    }

    public Task<ServiceBusReceivedMessage?> TryPeekDeadLetteredAsync(string messageId,
        CancellationToken cancellationToken)
    {
        return TryPeekAsync(SubQueue.DeadLetter, messageId, cancellationToken);
    }

    private async Task<ServiceBusReceivedMessage?> TryPeekAsync(SubQueue subQueue, string messageId,
        CancellationToken cancellationToken)
    {
        await using var receiver = _client.CreateReceiver(queueName,
            new ServiceBusReceiverOptions { SubQueue = subQueue });

        var messages = await receiver.PeekMessagesAsync(MaxPeekedMessages, fromSequenceNumber: 0,
            cancellationToken: cancellationToken);

        return messages.FirstOrDefault(m => m.MessageId == messageId);
    }
}
