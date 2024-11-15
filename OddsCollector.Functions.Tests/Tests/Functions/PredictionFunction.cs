﻿using NSubstitute.ReceivedExtensions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Strategies;
using OddsCollector.Functions.Tests.Infrastructure.CancellationToken;
using OddsCollector.Functions.Tests.Infrastructure.Data;
using OddsCollector.Functions.Tests.Infrastructure.Logger;
using OddsCollector.Functions.Tests.Infrastructure.ServiceBus;

namespace OddsCollector.Functions.Tests.Tests.Functions;

internal class PredictionFunction
{
    [Test]
    public async Task Run_WithValidServiceBusMessage_ReturnsEventPrediction()
    {
        // Arrange
        var loggerStub = LoggerFactory.GetLoggerMock<OddsCollector.Functions.Functions.PredictionFunction>();

        var upcomingEvent = new UpcomingEventBuilder().SetSampleData().Instance;

        var expectedPrediction = new EventPredictionBuilder().SetSampleData().Instance;

        var receivedMessages =
            ServiceBusReceivedMessageFactory.CreateFromObjects([
                upcomingEvent
            ]).ToArray();

        var actionsMock = ServiceBusMessageActionsFactory.GetServiceBusMessageActionsMock();

        var strategyStub = Substitute.For<IPredictionStrategy>();
        strategyStub.GetPrediction(Arg.Any<UpcomingEvent>(), Arg.Any<DateTime>()).Returns(expectedPrediction);

        var function = new OddsCollector.Functions.Functions.PredictionFunction(loggerStub, strategyStub);

        var token = new CancellationToken();

        // Act
        var prediction = await function.Run(receivedMessages, actionsMock, token).ConfigureAwait(false);

        // Assert
        prediction.Should().NotBeNull().And.HaveCount(1);
        prediction[0].Should().NotBeNull().And.Be(expectedPrediction);

        await actionsMock.Received(Quantity.Exactly(1)).CompleteMessageAsync(receivedMessages[0], token);
    }

    [Test]
    public async Task Run_WithValidServiceBusMessageAndRequestedCancellation_ReturnsNoPredictions()
    {
        // Arrange
        var loggerStub = LoggerFactory.GetLoggerMock<OddsCollector.Functions.Functions.PredictionFunction>();

        var upcomingEvent = new UpcomingEventBuilder().SetSampleData().Instance;

        var expectedPrediction = new EventPredictionBuilder().SetSampleData().Instance;

        var receivedMessages =
            ServiceBusReceivedMessageFactory.CreateFromObjects([
                upcomingEvent
            ]).ToArray();

        var actionsMock = ServiceBusMessageActionsFactory.GetServiceBusMessageActionsMock();

        var strategyStub = Substitute.For<IPredictionStrategy>();
        strategyStub.GetPrediction(Arg.Any<UpcomingEvent>(), Arg.Any<DateTime>()).Returns(expectedPrediction);

        var function = new OddsCollector.Functions.Functions.PredictionFunction(loggerStub, strategyStub);

        var cancellationToken = await CancellationTokenGenerator.GetRequestedForCancellationToken();

        // Act
        var prediction = await function.Run(receivedMessages, actionsMock, cancellationToken)
            .ConfigureAwait(false);

        // Assert
        prediction.Should().NotBeNull().And.HaveCount(0);
    }

    [Test]
    public async Task Run_WithInvalidItems_ReturnsSuccessfullyProcessedEventPredictions()
    {
        // Arrange
        var loggerMock = LoggerFactory.GetLoggerMock<OddsCollector.Functions.Functions.PredictionFunction>();

        var goodUpcomingEvent = new UpcomingEventBuilder().SetSampleData().Instance;

        var badUpcomingEvent = new UpcomingEventBuilder().SetSampleData()
            .SetAwayTeam(SampleEvent.HomeTeam).Instance;

        var expectedPrediction = new EventPredictionBuilder().SetSampleData().Instance;

        var receivedMessages =
            ServiceBusReceivedMessageFactory.CreateFromObjects([
                badUpcomingEvent,
                goodUpcomingEvent
            ]).ToArray();

        var actionsMock = ServiceBusMessageActionsFactory.GetServiceBusMessageActionsMock();

        var exception = new Exception();

        var strategyStub = Substitute.For<IPredictionStrategy>();
        strategyStub.GetPrediction(Arg.Any<UpcomingEvent>(), Arg.Any<DateTime>())
            .Returns(_ => throw exception, _ => expectedPrediction);

        var function = new OddsCollector.Functions.Functions.PredictionFunction(loggerMock, strategyStub);

        var token = new CancellationToken();

        // Act
        var prediction = await function.Run(receivedMessages, actionsMock, token).ConfigureAwait(false);

        // Assert
        prediction.Should().NotBeNull().And.HaveCount(1);
        prediction[0].Should().NotBeNull().And.Be(expectedPrediction);

        await actionsMock.Received(Quantity.Exactly(1)).CompleteMessageAsync(receivedMessages[1], token);

        var receivedCalls = loggerMock.ReceivedCalls().ToList();

        receivedCalls.Should().NotBeNull().And.HaveCount(1);

        var firstReceived = receivedCalls.First();

        var firstReceivedArguments = firstReceived.GetArguments();

        firstReceivedArguments.Should().NotBeNull();
        firstReceivedArguments[3].Should().NotBeNull().And.Be(exception);
    }
}
