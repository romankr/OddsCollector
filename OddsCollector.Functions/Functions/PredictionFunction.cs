using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Processors;

namespace OddsCollector.Functions.Functions;

internal sealed class PredictionFunction(ILogger<PredictionFunction> logger, IPredictionProcessor processor)
{
    [Function(nameof(PredictionFunction))]
    [CosmosDBOutput("%CosmosDb:Database%", "%CosmosDb:EventPredictionsContainer%",
        Connection = "CosmosDb:Connection")]
    public async Task<EventPrediction[]> Run(
        [ServiceBusTrigger("%ServiceBus:Queue%", Connection = "ServiceBus:Connection", IsBatched = true)]
        ServiceBusReceivedMessage[] messages, ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        try
        {
            return await processor.ProcessMessagesAsync(messages, messageActions, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to make predictions");
        }

        return [];
    }
}
