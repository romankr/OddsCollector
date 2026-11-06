using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.Functions;

internal sealed partial class PredictionFunction(
    ILogger<PredictionFunction> logger,
    IPredictionStrategy strategy,
    TimeProvider timeProvider)
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
        UpcomingEvent @event;

        try
        {
            @event = message.Body.ToObjectFromJson<UpcomingEvent>()
                     ?? throw new JsonException("Message body is null");
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException)
        {
            LogDeadLettering(logger, exception, message.MessageId);

            await messageActions.DeadLetterMessageAsync(message,
                deadLetterReason: exception.GetType().Name,
                deadLetterErrorDescription: exception.Message,
                cancellationToken: cancellationToken);

            return null;
        }

        if (@event.CommenceTime <= timeProvider.GetUtcNow().UtcDateTime)
        {
            LogStartedEventSkipped(logger, @event.Id);

            return null;
        }

        if (!strategy.TryGetPrediction(@event, out var prediction))
        {
            LogUnpredictableEventSkipped(logger, @event.Id);

            return null;
        }

        return prediction;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Skipped event {Id}: already started")]
    private static partial void LogStartedEventSkipped(ILogger logger, string id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Skipped event {Id}: no usable odds")]
    private static partial void LogUnpredictableEventSkipped(ILogger logger, string id);

    [LoggerMessage(Level = LogLevel.Error, Message = "Dead-lettering message {MessageId}")]
    private static partial void LogDeadLettering(ILogger logger, Exception exception, string messageId);
}
