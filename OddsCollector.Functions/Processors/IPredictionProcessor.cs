using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Processors;

internal interface IPredictionProcessor
{
    Task<EventPrediction[]> ProcessMessagesAsync(ServiceBusReceivedMessage[] messages, ServiceBusMessageActions messageActions, CancellationToken cancellationToken);
}
