using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using OddsCollector.Common.Models;
using OddsCollector.Functions.Predictions.Strategies;

namespace OddsCollector.Functions.Predictions.Tests;

[Parallelizable(ParallelScope.All)]
internal class PredictionFunctionTests
{
    [Test]
    public void Constructor_WithValidDependencies_ReturnsNewInstance()
    {
        var loggerStub = Substitute.For<ILogger<PredictionFunction>>();
        var strategyStub = Substitute.For<IPredictionStrategy>();

        var function = new PredictionFunction(loggerStub, strategyStub);

        function.Should().NotBeNull();
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowException()
    {
        var strategyStub = Substitute.For<IPredictionStrategy>();

        var action = () =>
        {
            _ = new PredictionFunction(null, strategyStub);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void Constructor_WithNullStrategy_ThrowException()
    {
        var loggerStub = Substitute.For<ILogger<PredictionFunction>>();

        var action = () =>
        {
            _ = new PredictionFunction(loggerStub, null);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("strategy");
    }

    [Test]
    public async Task Run_WithServiceBusMessage_ReturnsEventPrediction()
    {
        var loggerStub = Substitute.For<ILogger<PredictionFunction>>();

        var upcomingEvent = new UpcomingEventBuilder()
            .SetAwayTeam("Liverpool")
            .SetCommenceTime(new DateTime(2023, 11, 25, 12, 30, 0))
            .SetHomeTeam("Manchester City")
            .SetId("4acd8f2675ca847ba33eea3664f6c0bb")
            .SetTraceId(Guid.NewGuid())
            .SetTimestamp(DateTime.Now)
            .SetOdds([
                new OddBuilder().SetAway(4.08).SetBookmaker("betclic").SetDraw(3.82).SetHome(1.7).Instance
            ]).Instance;

        var expectedPrediction = new EventPredictionBuilder()
            .SetId("4acd8f2675ca847ba33eea3664f6c0bb")
            .SetAwayTeam("Liverpool")
            .SetBookmaker("betclic")
            .SetCommenceTime(new DateTime(2023, 11, 25, 12, 30, 0))
            .SetHomeTeam("Manchester City")
            .SetStrategy(nameof(AdjustedConsensusStrategy))
            .SetTraceId(Guid.NewGuid())
            .SetTimestamp(DateTime.Now)
            .SetWinner("Manchester City").Instance;

        ServiceBusReceivedMessage[] receivedmessages =
            [ServiceBusReceivedMessageFactory.CreateFromObject(upcomingEvent)];

        var actionsMock = Substitute.For<ServiceBusMessageActions>();

        var strategyStub = Substitute.For<IPredictionStrategy>();
        strategyStub.GetPrediction(Arg.Any<UpcomingEvent>(), Arg.Any<DateTime>()).Returns(expectedPrediction);

        var function = new PredictionFunction(loggerStub, strategyStub);

        var token = new CancellationToken();

        var prediction = await function.Run(receivedmessages, actionsMock, token).ConfigureAwait(false);

        prediction.Should().NotBeNull().And.HaveCount(1);
        prediction[0].Should().NotBeNull().And.Be(expectedPrediction);

        await actionsMock.Received(Quantity.Exactly(1)).CompleteMessageAsync(receivedmessages[0], token);
    }

    [Test]
    public async Task Run_WithInvalidItems_ReturnsSucessfuallyProcessedEventPredictions()
    {
        var loggerMock = Substitute.For<ILogger<PredictionFunction>>();

        var goodUpcomingEvent = new UpcomingEventBuilder()
            .SetAwayTeam("Liverpool")
            .SetCommenceTime(new DateTime(2023, 11, 25, 12, 30, 0))
            .SetHomeTeam("Manchester City")
            .SetId("4acd8f2675ca847ba33eea3664f6c0bb")
            .SetTraceId(Guid.NewGuid())
            .SetTimestamp(DateTime.Now)
            .SetOdds([
                new OddBuilder().SetAway(4.08).SetBookmaker("betclic").SetDraw(3.82).SetHome(1.7).Instance
            ]).Instance;

        var badUpcomingEvent = new UpcomingEventBuilder()
            .SetAwayTeam("Wolverhampton Wanderers")
            .SetCommenceTime(new DateTime(2023, 12, 2, 15, 0, 0))
            .SetHomeTeam("Arsenal")
            .SetId("4dbfa65a0679bdcda9b61afa7a7e3c73")
            .SetTraceId(Guid.NewGuid())
            .SetTimestamp(DateTime.Now)
            .SetOdds([
                new OddBuilder().SetAway(4.08).SetBookmaker("betclic").SetDraw(3.82).SetHome(1.7).Instance
            ]).Instance;

        var expectedPrediction = new EventPredictionBuilder()
            .SetId("4acd8f2675ca847ba33eea3664f6c0bb")
            .SetAwayTeam("Liverpool")
            .SetBookmaker("betclic")
            .SetCommenceTime(new DateTime(2023, 11, 25, 12, 30, 0))
            .SetHomeTeam("Manchester City")
            .SetStrategy(nameof(AdjustedConsensusStrategy))
            .SetTraceId(Guid.NewGuid())
            .SetTimestamp(DateTime.Now)
            .SetWinner("Manchester City").Instance;

        ServiceBusReceivedMessage[] receivedmessages =
        [
            ServiceBusReceivedMessageFactory.CreateFromObject(badUpcomingEvent),
            ServiceBusReceivedMessageFactory.CreateFromObject(goodUpcomingEvent)
        ];

        var actionsMock = Substitute.For<ServiceBusMessageActions>();

        var exception = new Exception();

        var strategyStub = Substitute.For<IPredictionStrategy>();
        strategyStub.GetPrediction(Arg.Any<UpcomingEvent>(), Arg.Any<DateTime>())
            .Returns(x => { throw exception; }, x => expectedPrediction);

        var function = new PredictionFunction(loggerMock, strategyStub);

        var token = new CancellationToken();

        var prediction = await function.Run(receivedmessages, actionsMock, token).ConfigureAwait(false);

        prediction.Should().NotBeNull().And.HaveCount(1);
        prediction[0].Should().NotBeNull().And.Be(expectedPrediction);

        await actionsMock.Received(Quantity.Exactly(1)).CompleteMessageAsync(receivedmessages[1], token);

        var receivedCalls = loggerMock.ReceivedCalls().ToList();

        receivedCalls.Should().NotBeNull().And.HaveCount(1);

        var firstReceived = receivedCalls.First();

        var firstReceivedArguments = firstReceived.GetArguments();

        firstReceivedArguments.Should().NotBeNull();
        firstReceivedArguments[3].Should().NotBeNull().And.Be(exception);
    }
}
