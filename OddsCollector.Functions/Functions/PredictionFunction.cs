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
        // AutoCompleteMessages stays on even though an unreadable message is dead-lettered below: the Service Bus
        // processor completes only messages that are not settled yet, so a dead-lettered message is not completed
        // a second time (see EndToEnd.PredictionFunction_UnreadableMessage_DeadLettersItWithoutSettlingItAgain).
        // Turning it off would mean completing the message in code before the Cosmos DB output binding writes
        // the prediction, and losing it if that write fails.
        [ServiceBusTrigger("%ServiceBus:Queue%", Connection = "ServiceBus:Connection",
            AutoCompleteMessages = true)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        try
        {
            var @event = message.Body.ToObjectFromJson<UpcomingEvent>()
                         ?? throw new JsonException("Message body is null");

            if (@event.CommenceTime <= timeProvider.GetUtcNow().UtcDateTime)
            {
                logger.LogInformation("Skipped event {Id}: already started", @event.Id);

                return null;
            }

            if (!strategy.TryGetPrediction(@event, out var prediction))
            {
                // A readable event without usable odds is a normal case, not a broken message: the host completes
                // it, and the next collection run sends the event again with fresh odds.
                logger.LogInformation("Skipped event {Id}: no usable odds", @event.Id);

                return null;
            }

            return prediction;
        }
        // Only a message that cannot be read as an upcoming event is dead-lettered.
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
