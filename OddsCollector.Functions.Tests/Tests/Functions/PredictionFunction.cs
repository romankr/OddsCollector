using Azure.Messaging.ServiceBus;
using FluentAssertions.Execution;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Processors;
using FunctionsApp = OddsCollector.Functions.Functions;

namespace OddsCollector.Functions.Tests.Tests.Functions;

internal sealed class PredictionFunction
{
    [Test]
    public async Task Run_WithServiceBusMessage_ReturnsEventPrediction()
    {
        // Arrange
        var loggerStub = new FakeLogger<FunctionsApp.PredictionFunction>();

        var expectedPrediction = new EventPrediction();
        EventPrediction[] expectedPredictions = [expectedPrediction];

        var processorStub = Substitute.For<IPredictionProcessor>();
        processorStub.ProcessMessagesAsync(Arg.Any<ServiceBusReceivedMessage[]>(),
            Arg.Any<ServiceBusMessageActions>(), Arg.Any<CancellationToken>()).Returns(expectedPredictions);

        var function = new FunctionsApp.PredictionFunction(loggerStub, processorStub);

        // Act
        var predictions = await function.Run([null!], null!, new CancellationToken()).ConfigureAwait(false);

        // Assert
        predictions.Should().NotBeNull().And.HaveCount(1);
        predictions[0].Should().NotBeNull().And.Be(expectedPrediction);
    }

    [Test]
    public async Task Run_WithException_ReturnsEmptyPredictionListAndLogsException()
    {
        // Arrange
        var loggerMock = new FakeLogger<FunctionsApp.PredictionFunction>();

        var exception = new Exception();

        var processorStub = Substitute.For<IPredictionProcessor>();
        processorStub.ProcessMessagesAsync(Arg.Any<ServiceBusReceivedMessage[]>(),
            Arg.Any<ServiceBusMessageActions>(), Arg.Any<CancellationToken>()).Throws(exception);

        var function = new FunctionsApp.PredictionFunction(loggerMock, processorStub);

        // Act
        var predictions = await function.Run([null!], null!, new CancellationToken()).ConfigureAwait(false);

        // Assert
        predictions.Should().NotBeNull().And.HaveCount(0);

        loggerMock.Collector.Count.Should().BeGreaterThanOrEqualTo(1);

        var logRecord = loggerMock.Collector.GetSnapshot()[0];

        using var scope = new AssertionScope();

        logRecord.Level.Should().Be(LogLevel.Error);
        logRecord.Message.Should().Be($"Failed to make predictions");
        logRecord.Exception.Should().Be(exception);
    }
}
