using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.Processors;

internal sealed class PredictionProcessor(ILogger<PredictionProcessor> logger, IPredictionStrategy strategy) : IPredictionProcessor
{
    public async Task<EventPrediction[]> ProcessMessagesAsync(ServiceBusReceivedMessage[] messages, ServiceBusMessageActions messageActions, CancellationToken cancellationToken)
    {
        var result = new List<EventPrediction>();

        foreach (var message in messages)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                var prediction = await GetPredictionAndCompleteMessageAsync(message, messageActions, cancellationToken);
                result.Add(prediction);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to processes message with id {Id}", message.MessageId);
            }
        }

        if (result.Count == 0)
        {
            logger.LogWarning("Processed 0 messages");
        }
        else
        {
            logger.LogInformation("Processed {Count} message(s)", result.Count);
        }

        return [.. result];
    }

    private async Task<EventPrediction> GetPredictionAndCompleteMessageAsync(ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions, CancellationToken cancellationToken)
    {
        var upcomingEvent = message.Body.ToObjectFromJson<UpcomingEvent>();

        var prediction = strategy.GetPrediction(upcomingEvent);

        await messageActions.CompleteMessageAsync(message, cancellationToken);

        return prediction;
    }
}
