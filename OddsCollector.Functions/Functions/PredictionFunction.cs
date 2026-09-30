using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.Functions;

internal sealed class PredictionFunction(ILogger<PredictionFunction> logger, IPredictionStrategy strategy)
{
    [Function(nameof(PredictionFunction))]
    [CosmosDBOutput("%CosmosDb:Database%", "%CosmosDb:EventPredictionsContainer%",
        Connection = "CosmosDb:Connection")]
    public async Task<EventPrediction?> Run(
        // The host completes the message only once the whole invocation has succeeded, the
        // Cosmos output binding included, and leaves alone a message settled in here.
        [ServiceBusTrigger("%ServiceBus:Queue%", Connection = "ServiceBus:Connection",
            AutoCompleteMessages = true)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        try
        {
            var @event = message.Body.ToObjectFromJson<UpcomingEvent>();

            return strategy.GetPrediction(@event);
        }
        // A message that cannot be read, or holds an event nothing can be predicted from,
        // fails the same way on every delivery, so it is dead-lettered at once instead of
        // being retried up to the delivery limit. Anything else may pass on a retry and is
        // left to fail the invocation, so the host abandons the message for redelivery.
        catch (Exception exception) when (exception is JsonException or ArgumentException)
        {
            logger.LogError(exception, "Dead-lettering message {MessageId}", message.MessageId);

            await messageActions.DeadLetterMessageAsync(message,
                deadLetterReason: exception.GetType().Name,
                deadLetterErrorDescription: exception.Message,
                cancellationToken: cancellationToken);

            // Nothing to store: a null return writes no document.
            return null;
        }
    }
}
