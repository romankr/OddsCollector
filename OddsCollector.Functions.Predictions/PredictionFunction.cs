using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using OddsCollector.Common.Models;
using OddsCollector.Functions.Predictions.Strategies;

namespace OddsCollector.Functions.Predictions;

internal class PredictionFunction
{
    private readonly IPredictionStrategy _strategy;

    public PredictionFunction(IPredictionStrategy strategy)
    {
        _strategy = strategy;
    }

    [Function(nameof(PredictionFunction))]
    [CosmosDBOutput("%CosmosDb:Database%", "%CosmosDb:Container%", Connection = "CosmosDb:Connection")]
    public async Task<EventPrediction> Run(
        [ServiceBusTrigger("%ServiceBus:Queue%", Connection = "ServiceBus:Connection")]
        ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        var upcomingEvent = message.Body.ToObjectFromJson<UpcomingEvent>();

        var prediction = _strategy.GetPrediction(upcomingEvent, DateTime.UtcNow);

        await messageActions.CompleteMessageAsync(message).ConfigureAwait(false);

        return prediction;
    }
}
