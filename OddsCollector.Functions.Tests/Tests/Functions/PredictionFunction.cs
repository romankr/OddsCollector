using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NSubstitute.ReceivedExtensions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Strategies;
using OddsCollector.Functions.Tests.Infrastructure.Data;
using OddsCollector.Functions.Tests.Infrastructure.ServiceBus;

namespace OddsCollector.Functions.Tests.Tests.Functions;

internal class PredictionFunction
{
    [Test]
    public async Task Run_WithValidServiceBusMessage_ReturnsEventPrediction()
    {
        // Arrange
        var loggerStub = Substitute.For<ILogger<OddsCollector.Functions.Functions.PredictionFunction>>();

        var upcomingEvent = new UpcomingEventBuilder().SetSampleData().Instance;

        var expectedPrediction = new EventPredictionBuilder().SetSampleData().Instance;

        ServiceBusReceivedMessage[] receivedMessages =
            [ServiceBusReceivedMessageFactory.CreateFromObject(upcomingEvent)];

        var actionsMock = Substitute.For<ServiceBusMessageActions>();

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
    public async Task Run_WithInvalidItems_ReturnsSuccessfullyProcessedEventPredictions()
    {
        // Arrange
        var loggerMock = Substitute.For<ILogger<OddsCollector.Functions.Functions.PredictionFunction>>();

        var goodUpcomingEvent = new UpcomingEventBuilder().SetSampleData().Instance;

        var badUpcomingEvent = new UpcomingEventBuilder().SetSampleData()
            .SetAwayTeam(SampleEvent.HomeTeam).Instance;

        var expectedPrediction = new EventPredictionBuilder().SetSampleData().Instance;

        ServiceBusReceivedMessage[] receivedMessages =
        [
            ServiceBusReceivedMessageFactory.CreateFromObject(badUpcomingEvent),
            ServiceBusReceivedMessageFactory.CreateFromObject(goodUpcomingEvent)
        ];

        var actionsMock = Substitute.For<ServiceBusMessageActions>();

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
