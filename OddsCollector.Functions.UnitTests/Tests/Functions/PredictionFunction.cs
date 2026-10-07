using System.Text.Json;
using Azure.Messaging.ServiceBus;
using FluentAssertions.Execution;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Predictions;
using OddsCollector.Functions.Tests.Infrastructure.ServiceBus;
using OddsCollector.Functions.Tests.Infrastructure.Time;
using OddsCollector.Tests.Infrastructure.Models;
using FunctionApp = OddsCollector.Functions.Functions;

namespace OddsCollector.Functions.Tests.Tests.Functions;

internal sealed class PredictionFunction
{
    private const string EventId = "eventId";
    private const string MessageId = "messageId";

    [Test]
    public async Task Run_WithUpcomingEvent_ReturnsPrediction()
    {
        // Arrange
        var expectedPrediction = ValidModels.CreateEventPrediction() with { Id = EventId };

        var strategyStub = Substitute.For<IPredictionStrategy>();
        strategyStub.GetPrediction(Arg.Any<UpcomingEvent>()).Returns(expectedPrediction);

        var messageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var function = CreateFunction(strategyStub);

        var message = ServiceBusReceivedMessageFactory.CreateFromObject(CreateUpcomingEvent(minutesFromNow: 60));

        // Act
        var prediction = await function.Run(message, messageActionsMock, CancellationToken.None);

        // Assert: the host completes the message once the prediction has been stored.
        using var scope = new AssertionScope();

        prediction.Should().BeSameAs(expectedPrediction);
        messageActionsMock.ReceivedCalls().Should().BeEmpty();
    }

    [Test]
    public async Task Run_WithEventNothingCanBePredictedFrom_DeadLettersMessage()
    {
        // Arrange
        var strategyStub = Substitute.For<IPredictionStrategy>();
        strategyStub.GetPrediction(Arg.Any<UpcomingEvent>())
            .Throws(new ArgumentException("odds cannot be empty", "odds"));

        var messageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var loggerMock = new FakeLogger<FunctionApp.PredictionFunction>();

        var function = CreateFunction(strategyStub, loggerMock);

        var message =
            ServiceBusReceivedMessageFactory.CreateFromObject(CreateUpcomingEvent(minutesFromNow: 60), MessageId);

        // Act
        var prediction = await function.Run(message, messageActionsMock, CancellationToken.None);

        // Assert: it would fail the same way on every delivery.
        using var scope = new AssertionScope();

        prediction.Should().BeNull();

        await messageActionsMock.Received(1).DeadLetterMessageAsync(message, Arg.Any<Dictionary<string, object>?>(),
            nameof(ArgumentException), Arg.Is<string>(d => d.StartsWith("odds cannot be empty")),
            Arg.Any<CancellationToken>());

        loggerMock.Collector.GetSnapshot().Should().ContainSingle().Which.Should().BeEquivalentTo(
            new { Level = LogLevel.Error, Message = $"Dead-lettering message {MessageId}" });
    }

    [TestCase("not json", "", TestName = "Run_WithUnreadableBody_DeadLettersMessage")]
    [TestCase("null", "Message body is null", TestName = "Run_WithNullBody_DeadLettersMessage")]
    public async Task Run_WithBrokenBody_DeadLettersMessage(string body, string expectedDescription)
    {
        // Arrange
        var messageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var function = CreateFunction(Substitute.For<IPredictionStrategy>());

        var message = ServiceBusReceivedMessageFactory.CreateFromText(body);

        // Act
        var prediction = await function.Run(message, messageActionsMock, CancellationToken.None);

        // Assert
        using var scope = new AssertionScope();

        prediction.Should().BeNull();

        await messageActionsMock.Received(1).DeadLetterMessageAsync(message, Arg.Any<Dictionary<string, object>?>(),
            nameof(JsonException), Arg.Is<string>(d => d.StartsWith(expectedDescription)),
            Arg.Any<CancellationToken>());
    }

    [TestCase("2100-01-01T18:00:00", "Unspecified", TestName = "Run_WithUnspecifiedCommenceTime_DeadLettersMessage")]
    [TestCase("2100-01-01T18:00:00+03:00", "Local", TestName = "Run_WithOffsetCommenceTime_DeadLettersMessage")]
    public async Task Run_WithNonUtcCommenceTime_DeadLettersMessage(string commenceTime, string expectedKind)
    {
        // Arrange
        var messageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var strategyMock = Substitute.For<IPredictionStrategy>();

        var function = CreateFunction(strategyMock);

        var message = ServiceBusReceivedMessageFactory.CreateFromText(
            $$"""
              {"Id":"{{EventId}}","HomeTeam":"home","AwayTeam":"away","CommenceTime":"{{commenceTime}}","Odds":[]}
              """);

        // Act
        var prediction = await function.Run(message, messageActionsMock, CancellationToken.None);

        // Assert
        using var scope = new AssertionScope();

        prediction.Should().BeNull();

        strategyMock.ReceivedCalls().Should().BeEmpty();

        await messageActionsMock.Received(1).DeadLetterMessageAsync(message, Arg.Any<Dictionary<string, object>?>(),
            nameof(ArgumentException),
            Arg.Is<string>(d => d.StartsWith($"CommenceTime must be UTC. Actual kind: {expectedKind}")),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Run_WithOtherException_LetsItReachTheHost()
    {
        // Arrange
        var expectedException = new InvalidOperationException();

        var strategyStub = Substitute.For<IPredictionStrategy>();
        strategyStub.GetPrediction(Arg.Any<UpcomingEvent>()).Throws(expectedException);

        var messageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var function = CreateFunction(strategyStub);

        var message = ServiceBusReceivedMessageFactory.CreateFromObject(CreateUpcomingEvent(minutesFromNow: 60));

        // Act
        var action = () => function.Run(message, messageActionsMock, CancellationToken.None);

        // Assert: the host abandons the message so it can be redelivered.
        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(expectedException);
        messageActionsMock.ReceivedCalls().Should().BeEmpty();
    }

    [TestCase(0, TestName = "Run_WithEventStartingNow_SkipsIt")]
    [TestCase(-90, TestName = "Run_WithEventInPlay_SkipsIt")]
    public async Task Run_WithStartedEvent_SkipsIt(int minutesFromNow)
    {
        // Arrange
        var strategyMock = Substitute.For<IPredictionStrategy>();

        var messageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var loggerMock = new FakeLogger<FunctionApp.PredictionFunction>();

        var function = CreateFunction(strategyMock, loggerMock);

        var message = ServiceBusReceivedMessageFactory.CreateFromObject(CreateUpcomingEvent(minutesFromNow));

        // Act
        var prediction = await function.Run(message, messageActionsMock, CancellationToken.None);

        // Assert: nothing is written, so the pre-match prediction stays, and the host completes the message.
        using var scope = new AssertionScope();

        prediction.Should().BeNull();
        strategyMock.ReceivedCalls().Should().BeEmpty();
        messageActionsMock.ReceivedCalls().Should().BeEmpty();
        loggerMock.Collector.GetSnapshot().Should().ContainSingle().Which.Should().BeEquivalentTo(
            new { Level = LogLevel.Information, Message = $"Skipped event {EventId}: already started" });
    }

    private static FunctionApp.PredictionFunction CreateFunction(IPredictionStrategy strategy,
        ILogger<FunctionApp.PredictionFunction>? logger = null)
    {
        return new FunctionApp.PredictionFunction(logger ?? NullLogger<FunctionApp.PredictionFunction>.Instance,
            strategy, FixedTimeProvider.AtNow);
    }

    private static UpcomingEvent CreateUpcomingEvent(int minutesFromNow)
    {
        return ValidModels.CreateUpcomingEvent() with
        {
            Id = EventId,
            CommenceTime = FixedTimeProvider.Now.UtcDateTime.AddMinutes(minutesFromNow),
            Odds = []
        };
    }
}
