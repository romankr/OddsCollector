using FluentAssertions.Execution;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReceivedExtensions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Predictions;
using OddsCollector.Functions.Tests.Infrastructure.CancellationToken;
using OddsCollector.Functions.Tests.Infrastructure.ServiceBus;
using FunctionApp = OddsCollector.Functions.Processors;

namespace OddsCollector.Functions.Tests.Tests.Processors;

internal sealed class PredictionProcessor
{
    [Test]
    public async Task ProcessMessagesAsync_WithServiceBusMessage_ReturnsPredictionAndCompletesMessageAndWritesLog()
    {
        // Arrange
        var actionsMock = Substitute.For<ServiceBusMessageActions>();

        var loggerMock = new FakeLogger<FunctionApp.PredictionProcessor>();

        var expectedPrediction = new EventPrediction();

        var message = ServiceBusReceivedMessageFactory.CreateFromObject(new UpcomingEvent());

        var strategyStub = Substitute.For<IPredictionStrategy>();
        strategyStub.GetPrediction(Arg.Any<UpcomingEvent>()).Returns(expectedPrediction);

        var cancellationToken = CancellationToken.None;

        var processor = new FunctionApp.PredictionProcessor(loggerMock, strategyStub);

        // Act
        var predictions = await processor.ProcessMessagesAsync([message], actionsMock, cancellationToken)
            .ConfigureAwait(false);

        // Assert
        predictions.Should().NotBeNull().And.NotBeEmpty().And.HaveCount(1);
        predictions[0].Should().NotBeNull().And.Be(expectedPrediction);

        await actionsMock.Received(Quantity.Exactly(1)).CompleteMessageAsync(message, cancellationToken);

        loggerMock.Collector.Count.Should().Be(1);

        using var scope = new AssertionScope();

        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Information);
        loggerMock.LatestRecord.Message.Should().Be("Processed 1 message(s)");
    }

    [Test]
    public async Task ProcessMessagesAsync_WithNoServiceBusMessages_ReturnsNoPredictionsAndWritesLog()
    {
        // Arrange
        var actionsMock = Substitute.For<ServiceBusMessageActions>();

        var loggerMock = new FakeLogger<FunctionApp.PredictionProcessor>();

        var strategyStub = Substitute.For<IPredictionStrategy>();

        var processor = new FunctionApp.PredictionProcessor(loggerMock, strategyStub);

        // Act
        var predictions = await processor.ProcessMessagesAsync([], actionsMock, CancellationToken.None)
            .ConfigureAwait(false);

        // Assert
        predictions.Should().NotBeNull().And.BeEmpty();

        actionsMock.Received(Quantity.Exactly(0));

        loggerMock.Collector.Count.Should().Be(1);

        using var scope = new AssertionScope();

        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Warning);
        loggerMock.LatestRecord.Message.Should().Be("Processed 0 messages");
    }

    [Test]
    public async Task ProcessMessagesAsync_WithException_ReturnsNoPredictionsAndWritesError()
    {
        // Arrange
        var actionsMock = Substitute.For<ServiceBusMessageActions>();

        var loggerMock = new FakeLogger<FunctionApp.PredictionProcessor>();

        const string expectedMessageId = "123";

        var message = ServiceBusReceivedMessageFactory.CreateFromObject(new UpcomingEvent(), expectedMessageId);

        var expectedException = new Exception();

        var strategyStub = Substitute.For<IPredictionStrategy>();
        strategyStub.GetPrediction(Arg.Any<UpcomingEvent>()).Throws(expectedException);

        var cancellationToken = CancellationToken.None;

        var processor = new FunctionApp.PredictionProcessor(loggerMock, strategyStub);

        // Act
        var predictions = await processor.ProcessMessagesAsync([message], actionsMock, cancellationToken)
            .ConfigureAwait(false);

        // Assert
        predictions.Should().NotBeNull().And.BeEmpty();

        actionsMock.Received(Quantity.Exactly(0));

        loggerMock.Collector.Count.Should().Be(2);

        using var scope = new AssertionScope();

        loggerMock.Collector.GetSnapshot().Count.Should().Be(2);

        var firstRecord = loggerMock.Collector.GetSnapshot()[0];

        firstRecord.Should().NotBeNull();
        firstRecord.Level.Should().Be(LogLevel.Error);
        firstRecord.Message.Should().Be("Failed to processes message with id 123");

        var secondRecord = loggerMock.Collector.GetSnapshot()[1];

        secondRecord.Should().NotBeNull();
        secondRecord.Level.Should().Be(LogLevel.Warning);
        secondRecord.Message.Should().Be("Processed 0 messages");
    }

    [Test]
    public async Task ProcessMessagesAsync_WithCancellationToken_ReturnsNoPredictions()
    {
        // Arrange
        var actionsMock = Substitute.For<ServiceBusMessageActions>();

        var loggerStub = new FakeLogger<FunctionApp.PredictionProcessor>();

        const string expectedMessageId = "123";

        var message = ServiceBusReceivedMessageFactory.CreateFromObject(new UpcomingEvent(), expectedMessageId);

        var expectedPrediction = new EventPrediction();

        var strategyStub = Substitute.For<IPredictionStrategy>();
        strategyStub.GetPrediction(Arg.Any<UpcomingEvent>()).Returns(expectedPrediction);

        var cancellationToken = await CancellationTokenGenerator.GetRequestedForCancellationToken();

        var processor = new FunctionApp.PredictionProcessor(loggerStub, strategyStub);

        // Act
        var predictions = await processor.ProcessMessagesAsync([message], actionsMock, cancellationToken)
            .ConfigureAwait(false);

        // Assert
        predictions.Should().NotBeNull().And.BeEmpty();

        actionsMock.Received(Quantity.Exactly(0));
    }
}
