using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.Functions;

internal sealed class PredictionFunction(
    ILogger<PredictionFunction> logger,
    IPredictionStrategy strategy,
    TimeProvider timeProvider)
{
    [Function(nameof(PredictionFunction))]
    [CosmosDBOutput("%CosmosDb:Database%", "%CosmosDb:EventPredictionsContainer%",
        Connection = "CosmosDb:Connection")]
    public async Task<EventPrediction?> Run(
        // AutoCompleteMessages stays on even though a bad message is dead-lettered below: the
        // Service Bus processor completes only messages that are not settled yet, so a dead-lettered
        // message is not completed a second time. Turning it off would mean completing the message
        // in code before the Cosmos DB output binding writes the prediction, and losing it if that
        // write fails.
        [ServiceBusTrigger("%ServiceBus:Queue%", Connection = "ServiceBus:Connection",
            AutoCompleteMessages = true)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        try
        {
            // A body of JSON null deserializes without an error, but there is no event to predict.
            var @event = message.Body.ToObjectFromJson<UpcomingEvent>()
                         ?? throw new JsonException("Message body is null");

            var commenceTime = @event.CommenceTime;

            // A message can wait in the queue past kick-off. The pre-match prediction stored
            // by an earlier message is kept rather than replaced by a late one.
            // Require throws for a time that is not UTC, as it cannot be compared with now. Such a
            // message fails the same way on every delivery, so it is dead-lettered below.
            if (UtcDateTime.Require(commenceTime) <= timeProvider.GetUtcNow().UtcDateTime)
            {
                logger.LogInformation("Skipped event {Id}: already started", @event.Id);

                return null;
            }

            return strategy.GetPrediction(@event);
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException)
        {
            logger.LogError(exception, "Dead-lettering message {MessageId}", message.MessageId);

            await messageActions.DeadLetterMessageAsync(message,
                deadLetterReason: exception.GetType().Name,
                deadLetterErrorDescription: exception.Message,
                cancellationToken: cancellationToken);

            return null;
        }
    }
}
