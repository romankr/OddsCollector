using System.Text;
using System.Text.Json;
using Azure.Core.Amqp;
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using OddsCollector.Common.Models;
using OddsCollector.Functions.Predictions.Strategies;

namespace OddsCollector.Functions.Predictions.Tests;

internal class PredictionFunctionTests
{
    [Test]
    public void Constructor_WithValidDependencies_ReturnsNewInstance()
    {
        var stub = Substitute.For<IPredictionStrategy>();

        var function = new PredictionFunction(stub);

        function.Should().NotBeNull();
    }

    [Test]
    public void Constructor_WithNullStrategy_ThrowException()
    {
        var action = () =>
        {
            _ = new PredictionFunction(null);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("strategy");
    }

    [Test]
    public async Task Run_WithServiceBusMessage_ReturnsEventPrediction()
    {
        var upcomingEvent = new UpcomingEvent
        {
            AwayTeam = "Liverpool",
            CommenceTime = new DateTime(2023, 11, 25, 12, 30, 0),
            HomeTeam = "Manchester City",
            Id = "4acd8f2675ca847ba33eea3664f6c0bb",
            TraceId = Guid.NewGuid(),
            Timestamp = DateTime.Now,
            Odds = [new() { Away = 4.08, Bookmaker = "betclic", Draw = 3.82, Home = 1.7 }]
        };

        var expectedPrediction = new EventPrediction
        {
            AwayTeam = "Liverpool",
            Bookmaker = "betclic",
            CommenceTime = new DateTime(2023, 11, 25, 12, 30, 0),
            HomeTeam = "Manchester City",
            Id = "4acd8f2675ca847ba33eea3664f6c0bb",
            Strategy = nameof(AdjustedConsensusStrategy),
            Timestamp = DateTime.Now,
            TraceId = Guid.NewGuid(),
            Winner = "Manchester City"
        };

        var serialized = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(upcomingEvent))
            .Select(x => new ReadOnlyMemory<byte>([x]));

        var ampqmessage = new AmqpMessageBody(serialized);

        var ampqannotatedmessage = new AmqpAnnotatedMessage(ampqmessage);

        var receivedmessage =
            ServiceBusReceivedMessage.FromAmqpMessage(ampqannotatedmessage, new BinaryData(Array.Empty<byte>()));

        var actionsMock = Substitute.For<ServiceBusMessageActions>();

        var strategyStub = Substitute.For<IPredictionStrategy>();
        strategyStub.GetPrediction(Arg.Any<UpcomingEvent>(), Arg.Any<DateTime>()).Returns(expectedPrediction);

        var function = new PredictionFunction(strategyStub);

        var prediction = await function.Run(receivedmessage, actionsMock).ConfigureAwait(false);

        prediction.Should().NotBeNull().And.Be(expectedPrediction);

        await actionsMock.Received(Quantity.Exactly(1)).CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>());
    }
}
