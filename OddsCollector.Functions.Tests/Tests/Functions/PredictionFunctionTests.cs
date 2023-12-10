using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NSubstitute.ReceivedExtensions;
using OddsCollector.Functions.Functions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Strategies;
using OddsCollector.Functions.Tests.Common.Models;
using OddsCollector.Functions.Tests.Common.ServiceBus;

namespace OddsCollector.Functions.Tests.Tests.Functions;

[Parallelizable(ParallelScope.All)]
internal class PredictionFunctionTests
{
    [Test]
    public async Task Run_WithValidServiceBusMessage_ReturnsEventPrediction()
    {
        var loggerStub = Substitute.For<ILogger<PredictionFunction>>();

        var upcomingEvent = new UpcomingEventBuilder().SetDefaults().Instance;

        var expectedPrediction = new EventPredictionBuilder().SetDefaults().Instance;

        ServiceBusReceivedMessage[] receivedMessages =
            [ServiceBusReceivedMessageFactory.CreateFromObject(upcomingEvent)];

        var actionsMock = Substitute.For<ServiceBusMessageActions>();

        var strategyStub = Substitute.For<IPredictionStrategy>();
        strategyStub.GetPrediction(Arg.Any<UpcomingEvent>(), Arg.Any<DateTime>()).Returns(expectedPrediction);

        var function = new PredictionFunction(loggerStub, strategyStub);

        var token = new CancellationToken();

        var prediction = await function.Run(receivedMessages, actionsMock, token).ConfigureAwait(false);

        prediction.Should().NotBeNull().And.HaveCount(1);
        prediction[0].Should().NotBeNull().And.Be(expectedPrediction);

        await actionsMock.Received(Quantity.Exactly(1)).CompleteMessageAsync(receivedMessages[0], token);
    }

    [Test]
    public async Task Run_WithInvalidItems_ReturnsSuccessfullyProcessedEventPredictions()
    {
        var loggerMock = Substitute.For<ILogger<PredictionFunction>>();

        var goodUpcomingEvent = new UpcomingEventBuilder().SetDefaults().Instance;

        var badUpcomingEvent = new UpcomingEventBuilder().SetDefaults()
            .SetAwayTeam(UpcomingEventBuilderExtensions.DefaultHomeTeam).Instance;

        var expectedPrediction = new EventPredictionBuilder().SetDefaults().Instance;

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

        var function = new PredictionFunction(loggerMock, strategyStub);

        var token = new CancellationToken();

        var prediction = await function.Run(receivedMessages, actionsMock, token).ConfigureAwait(false);

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
