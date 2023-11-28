using System.Runtime.CompilerServices;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using OddsCollector.Common.Models;
using OddsCollector.Functions.Predictions.Strategies;

[assembly: InternalsVisibleTo("OddsCollector.Functions.Predictions.Tests")]
// DynamicProxyGenAssembly2 is a temporary assembly built by mocking systems that use CastleProxy
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace OddsCollector.Functions.Predictions;

internal sealed class PredictionFunction(IPredictionStrategy? strategy)
{
    private readonly IPredictionStrategy _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));

    [Function(nameof(PredictionFunction))]
    [CosmosDBOutput("%CosmosDb:Database%", "%CosmosDb:Container%", Connection = "CosmosDb:Connection")]
    public async Task<EventPrediction[]> Run(
        [ServiceBusTrigger("%ServiceBus:Queue%", Connection = "ServiceBus:Connection", IsBatched = true)]
        ServiceBusReceivedMessage[] messages, ServiceBusMessageActions messageActions, CancellationToken cancellationToken)
    {
        EventPrediction[] predictions = new EventPrediction[messages.Length];

        for (int i = 0; i < messages.Length; i++)
        {
            var upcomingEvent = messages[i].Body.ToObjectFromJson<UpcomingEvent>();

            predictions[i] = _strategy.GetPrediction(upcomingEvent, DateTime.UtcNow);

            await messageActions.CompleteMessageAsync(messages[i], cancellationToken).ConfigureAwait(false);
        }

        return predictions;
    }
}
