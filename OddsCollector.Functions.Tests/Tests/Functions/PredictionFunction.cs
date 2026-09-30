using System.Text.Json;
using Azure.Messaging.ServiceBus;
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
            NullLogger<FunctionApp.PredictionFunction>.Instance, strategyStub);

        // Act
        var prediction = await function.Run(ServiceBusReceivedMessageFactory.CreateFromObject(new UpcomingEvent()),
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

        var function = new FunctionApp.PredictionFunction(loggerMock, strategyStub);

        var message = ServiceBusReceivedMessageFactory.CreateFromObject(new UpcomingEvent(), "messageId");

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

    [TestCase("not json", TestName = "Run_WithUnreadableBody_DeadLettersMessage")]
    [TestCase("null", TestName = "Run_WithNullBody_DeadLettersMessage")]
    public async Task Run_WithBrokenBody_DeadLettersMessage(string body)
    {
        // Arrange
        var strategy = new PredictionStrategy(Substitute.For<IOutcomePredictor>());

        var messageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var function = new FunctionApp.PredictionFunction(
            NullLogger<FunctionApp.PredictionFunction>.Instance, strategy);

        var message = ServiceBusReceivedMessageFactory.CreateFromText(body);

        // Act
        var prediction = await function.Run(message, messageActionsMock, CancellationToken.None);

        // Assert
        prediction.Should().BeNull();

        await messageActionsMock.Received(1).DeadLetterMessageAsync(message, Arg.Any<Dictionary<string, object>?>(),
            Arg.Is<string>(r => r == nameof(JsonException) || r == nameof(ArgumentNullException)), Arg.Any<string>(),
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
            NullLogger<FunctionApp.PredictionFunction>.Instance, strategyStub);

        // Act
        var action = () => function.Run(ServiceBusReceivedMessageFactory.CreateFromObject(new UpcomingEvent()),
            messageActionsMock, CancellationToken.None);

        // Assert: the host abandons the message so it can be redelivered.
        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(expectedException);

        messageActionsMock.ReceivedCalls().Should().BeEmpty();
    }
}
