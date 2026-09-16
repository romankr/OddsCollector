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
    public EventPrediction Run(
        // The host settles the message, and only once the whole invocation has succeeded -
        // the Cosmos output binding included. Settling in here would release the message
        // before the prediction was stored, and a failed write would lose it silently.
        [ServiceBusTrigger("%ServiceBus:Queue%", Connection = "ServiceBus:Connection",
            AutoCompleteMessages = true)]
        ServiceBusReceivedMessage message)
    {
        // Deliberately not caught. The host abandons the message on a failure, so it is
        // redelivered and eventually dead-lettered; swallowing here would have the host
        // complete a message whose prediction was never written.
        var upcomingEvent = message.Body.ToObjectFromJson<UpcomingEvent>();

        var prediction = strategy.GetPrediction(upcomingEvent);

        logger.LogInformation("Predicted {Winner} for event {Id}", prediction.Winner, prediction.Id);

        return prediction;
    }
}
