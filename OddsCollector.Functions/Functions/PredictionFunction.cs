using System.Runtime.CompilerServices;
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
        List<EventPrediction> predictions = [];

        await foreach (var prediction in ProcessMessagesAsync(messages, messageActions, cancellationToken))
        {
            predictions.Add(prediction);
        }

        if (predictions.Count == 0)
        {
            logger.LogWarning("Processed 0 messages");
        }
        else
        {
            logger.LogInformation("Processed {Count} message(s)", predictions.Count);
        }

        return [.. predictions];
    }

    private async IAsyncEnumerable<EventPrediction> ProcessMessagesAsync(ServiceBusReceivedMessage[] messages,
        ServiceBusMessageActions messageActions, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var message in messages)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            EventPrediction? prediction = null;

            try
            {
                prediction = await processor.DeserializeAndCompleteMessageAsync(message, messageActions, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to processes message with id {Id}", message.MessageId);
            }

            if (prediction is not null)
            {
                yield return prediction;
            }
        }
    }
}
