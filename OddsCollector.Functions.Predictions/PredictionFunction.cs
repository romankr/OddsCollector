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
    public async Task<EventPrediction> Run(
        [ServiceBusTrigger("%ServiceBus:Queue%", Connection = "ServiceBus:Connection")]
        ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions, CancellationToken cancellationToken)
    {
        var upcomingEvent = message.Body.ToObjectFromJson<UpcomingEvent>();

        var prediction = _strategy.GetPrediction(upcomingEvent, DateTime.UtcNow);

        await messageActions.CompleteMessageAsync(message, cancellationToken).ConfigureAwait(false);

        return prediction;
    }
}
