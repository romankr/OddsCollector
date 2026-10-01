using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Predictions;
using OddsCollector.Functions.Tests.Infrastructure.ServiceBus;
using FunctionApp = OddsCollector.Functions.Functions;

namespace OddsCollector.Functions.Tests.Tests.Functions;

internal sealed class PredictionFunction
{
    [Test]
    public async Task Run_WithServiceBusMessage_ReturnsPrediction()
    {
        // Arrange
        var expectedPrediction = new EventPrediction { Id = "id", Outcome = OutcomeTypes.HomeTeam };

        var strategyStub = Substitute.For<IPredictionStrategy>();
        strategyStub.GetPrediction(Arg.Any<UpcomingEvent>()).Returns(expectedPrediction);

        var messageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var function = new FunctionApp.PredictionFunction(
            NullLogger<FunctionApp.PredictionFunction>.Instance, strategyStub, FixedTimeProvider.AtNow);

        // Act
        var prediction = await function.Run(ServiceBusReceivedMessageFactory.CreateFromObject(CreateUpcomingEvent()),
            messageActionsMock, CancellationToken.None);

        // Assert: the host completes the message once the prediction has been stored.
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

        var function = new FunctionApp.PredictionFunction(loggerMock, strategyStub, FixedTimeProvider.AtNow);

        var message = ServiceBusReceivedMessageFactory.CreateFromObject(CreateUpcomingEvent(), "messageId");

        // Act
        var prediction = await function.Run(message, messageActionsMock, CancellationToken.None);

        // Assert: it would fail the same way on every delivery.
        prediction.Should().BeNull();

        await messageActionsMock.Received(1).DeadLetterMessageAsync(message, Arg.Any<Dictionary<string, object>?>(),
            nameof(ArgumentException), Arg.Is<string>(d => d.StartsWith("odds cannot be empty")),
            Arg.Any<CancellationToken>());

        loggerMock.Collector.Count.Should().Be(1);
        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Error);
        loggerMock.LatestRecord.Message.Should().Be("Dead-lettering message messageId");
    }

    [TestCase("not json", "", TestName = "Run_WithUnreadableBody_DeadLettersMessage")]
    [TestCase("null", "Message body is null", TestName = "Run_WithNullBody_DeadLettersMessage")]
    public async Task Run_WithBrokenBody_DeadLettersMessage(string body, string expectedDescription)
    {
        // Arrange
        var strategy = new PredictionStrategy(Substitute.For<IOutcomePredictor>());

        var messageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var function = new FunctionApp.PredictionFunction(
            NullLogger<FunctionApp.PredictionFunction>.Instance, strategy, FixedTimeProvider.AtNow);

        var message = ServiceBusReceivedMessageFactory.CreateFromText(body);

        // Act
        var prediction = await function.Run(message, messageActionsMock, CancellationToken.None);

        // Assert
        prediction.Should().BeNull();

        await messageActionsMock.Received(1).DeadLetterMessageAsync(message, Arg.Any<Dictionary<string, object>?>(),
            nameof(JsonException), Arg.Is<string>(d => d.StartsWith(expectedDescription)),
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

        var function = new FunctionApp.PredictionFunction(
            NullLogger<FunctionApp.PredictionFunction>.Instance, strategyStub, FixedTimeProvider.AtNow);

        // Act
        var action = () => function.Run(ServiceBusReceivedMessageFactory.CreateFromObject(CreateUpcomingEvent()),
            messageActionsMock, CancellationToken.None);

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

        var function = new FunctionApp.PredictionFunction(loggerMock, strategyMock, FixedTimeProvider.AtNow);

        var @event = new UpcomingEvent
        {
            Id = "id",
            CommenceTime = FixedTimeProvider.Now.UtcDateTime.AddMinutes(minutesFromNow)
        };

        // Act
        var prediction = await function.Run(ServiceBusReceivedMessageFactory.CreateFromObject(@event),
            messageActionsMock, CancellationToken.None);

        // Assert: nothing is written, so the pre-match prediction stays, and the host
        // completes the message.
        prediction.Should().BeNull();

        strategyMock.ReceivedCalls().Should().BeEmpty();
        messageActionsMock.ReceivedCalls().Should().BeEmpty();

        loggerMock.Collector.Count.Should().Be(1);
        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Information);
        loggerMock.LatestRecord.Message.Should().Be("Skipped event id: already started");
    }

    private static UpcomingEvent CreateUpcomingEvent()
    {
        return new UpcomingEvent { CommenceTime = FixedTimeProvider.Now.UtcDateTime.AddHours(1) };
    }
}
