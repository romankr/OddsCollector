﻿using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.Processors;

internal class PredictionProcessor(IPredictionStrategy strategy) : IPredictionProcessor
{
    public async Task<EventPrediction> DeserializeAndCompleteMessageAsync(ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions, CancellationToken cancellationToken)
    {
        var upcomingEvent = message.Body.ToObjectFromJson<UpcomingEvent>();

        var prediction = strategy.GetPrediction(upcomingEvent, DateTime.UtcNow);

        await messageActions.CompleteMessageAsync(message, cancellationToken);

        return prediction;
    }
}
