using Azure.Messaging.ServiceBus;
using FluentAssertions.Execution;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Processors;
using OddsCollector.Functions.Tests.Infrastructure.CancellationToken;
using OddsCollector.Functions.Tests.Infrastructure.ServiceBus;

namespace OddsCollector.Functions.Tests.Tests.Functions;

internal class PredictionFunction
{
    [Test]
    public async Task Run_WithServiceBusMessage_ReturnsEventPredictionAndLogsCount()
    {
        // Arrange
        var loggerMock = new FakeLogger<OddsCollector.Functions.Functions.PredictionFunction>();

        var expectedPrediction = new EventPrediction();

        var processorStub = Substitute.For<IPredictionProcessor>();
        processorStub.DeserializeAndCompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(),
            Arg.Any<ServiceBusMessageActions>(), Arg.Any<CancellationToken>()).Returns(expectedPrediction);

        var function = new OddsCollector.Functions.Functions.PredictionFunction(loggerMock, processorStub);

        // Act
        var predictions = await function.Run([null!], null!, new CancellationToken()).ConfigureAwait(false);

        // Assert
        predictions.Should().NotBeNull().And.HaveCount(1);
        predictions[0].Should().NotBeNull().And.Be(expectedPrediction);

        loggerMock.Collector.Count.Should().Be(1);

        using (var scope = new AssertionScope())
        {
            loggerMock.LatestRecord.Level.Should().Be(LogLevel.Information);
            loggerMock.LatestRecord.Message.Should().Be("Processed 1 message(s)");
        }
    }

    [Test]
    public async Task Run_WithRequestedCancellation_ReturnsEmptyPredictionListAndLogsWarning()
    {
        // Arrange
        var loggerMock = new FakeLogger<OddsCollector.Functions.Functions.PredictionFunction>();

        var processorStub = Substitute.For<IPredictionProcessor>();
        processorStub.DeserializeAndCompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(),
            Arg.Any<ServiceBusMessageActions>(), Arg.Any<CancellationToken>())
            .Returns(new EventPrediction());

        var function = new OddsCollector.Functions.Functions.PredictionFunction(loggerMock, processorStub);

        var cancellationToken = await CancellationTokenGenerator.GetRequestedForCancellationToken();

        // Act
        var predictions = await function.Run([null!], null!, cancellationToken).ConfigureAwait(false);

        // Assert
        predictions.Should().NotBeNull().And.HaveCount(0);

        loggerMock.Collector.Count.Should().Be(1);

        using (var scope = new AssertionScope())
        {
            loggerMock.LatestRecord.Level.Should().Be(LogLevel.Warning);
            loggerMock.LatestRecord.Message.Should().Be("Processed 0 messages");
        }
    }

    [Test]
    public async Task Run_WithException_ReturnsEmptyPredictionListAndLogsException()
    {
        // Arrange
        var loggerMock = new FakeLogger<OddsCollector.Functions.Functions.PredictionFunction>();

        var exception = new Exception();

        const string expectedMessageId = "123";

        var message = ServiceBusReceivedMessageFactory.CreateFromObject(
            new UpcomingEvent(), expectedMessageId);

        var processorStub = Substitute.For<IPredictionProcessor>();
        processorStub.DeserializeAndCompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(),
            Arg.Any<ServiceBusMessageActions>(), Arg.Any<CancellationToken>()).Throws(exception);

        var function = new OddsCollector.Functions.Functions.PredictionFunction(loggerMock, processorStub);

        // Act
        var predictions = await function.Run([message], null!, new CancellationToken()).ConfigureAwait(false);

        // Assert
        predictions.Should().NotBeNull().And.HaveCount(0);

        loggerMock.Collector.Count.Should().BeGreaterThanOrEqualTo(1);

        var logRecord = loggerMock.Collector.GetSnapshot()[0];

        using (var scope = new AssertionScope())
        {
            logRecord.Level.Should().Be(LogLevel.Error);
            logRecord.Message.Should().Be($"Failed to processes message with id {expectedMessageId}");
            logRecord.Exception.Should().Be(exception);
        }
    }
}
