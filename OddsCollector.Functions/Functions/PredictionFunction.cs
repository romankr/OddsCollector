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
