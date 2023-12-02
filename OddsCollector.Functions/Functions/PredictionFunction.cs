using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Strategies;

namespace OddsCollector.Functions.Functions;

internal class PredictionFunction(ILogger<PredictionFunction>? logger, IPredictionStrategy? strategy)
{
    private readonly ILogger<PredictionFunction> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IPredictionStrategy _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));

    [Function(nameof(PredictionFunction))]
    [CosmosDBOutput("%CosmosDb:Database%", "%CosmosDb:EventPredictionsContainer%", Connection = "CosmosDb:Connection")]
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

                var prediction = _strategy.GetPrediction(upcomingEvent, DateTime.UtcNow);

                predictions.Add(prediction);

                await messageActions.CompleteMessageAsync(message, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to convert message with id {Id}", message.MessageId);
            }
        }

        return [.. predictions];
    }
}
