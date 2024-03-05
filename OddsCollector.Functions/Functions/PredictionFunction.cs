using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Strategies;

namespace OddsCollector.Functions.Functions;

internal class PredictionFunction(ILogger<PredictionFunction> logger, IPredictionStrategy strategy)
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

        foreach (var message in messages)
        {
            try
            {
                var upcomingEvent = message.Body.ToObjectFromJson<UpcomingEvent>();

                var prediction = strategy.GetPrediction(upcomingEvent, DateTime.UtcNow);

                predictions.Add(prediction);

                await messageActions.CompleteMessageAsync(message, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to convert message with id {Id}", message.MessageId);
            }
        }

        return [.. predictions];
    }
}
