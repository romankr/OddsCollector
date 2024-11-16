using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework.Internal;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Processors;

namespace OddsCollector.Functions.Tests.Tests.Functions;

internal class EventResultsFunction
{
    [Test]
    public async Task Run_WithValidMessages_ReturnsEventResultListAndLogsCount()
    {
        // Arrange
        IEnumerable<EventResult> expectedEventResults = [new()];

        var loggerMock = new FakeLogger<OddsCollector.Functions.Functions.EventResultsFunction>();

        var processorStub = Substitute.For<IEventResultProcessor>();

        processorStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedEventResults));

        var function = new OddsCollector.Functions.Functions.EventResultsFunction(loggerMock, processorStub);

        // Act
        var actualEventResults = await function.Run(new CancellationToken());

        // Assert
        actualEventResults.Should().NotBeNull().And.BeEquivalentTo(expectedEventResults);

        loggerMock.Collector.Count.Should().Be(1);
        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Information);
        loggerMock.LatestRecord.Message.Should().Be("1 event(s) received");
    }

    [Test]
    public async Task Run_WithException_ReturnsEmptyEventResultListAndLogsException()
    {
        // Arrange
        var exception = new Exception();

        var loggerMock = new FakeLogger<OddsCollector.Functions.Functions.EventResultsFunction>();

        var processorStub = Substitute.For<IEventResultProcessor>();

        processorStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Throws(exception);

        var function = new OddsCollector.Functions.Functions.EventResultsFunction(loggerMock, processorStub);

        // Act
        var actualEventResults = await function.Run(new CancellationToken());

        // Assert
        actualEventResults.Should().NotBeNull().And.BeEmpty();

        loggerMock.Collector.Count.Should().Be(1);
        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Error);
        loggerMock.LatestRecord.Message.Should().Be("Failed to get events");
        loggerMock.LatestRecord.Exception.Should().Be(exception);
    }

    [Test]
    public async Task Run_WithEmptyMessages_ReturnsEmptyEventResultListAndLogsWarning()
    {
        // Arrange
        IEnumerable<EventResult> expectedEventResults = [];

        var loggerMock = new FakeLogger<OddsCollector.Functions.Functions.EventResultsFunction>();

        var processorStub = Substitute.For<IEventResultProcessor>();

        processorStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedEventResults));

        var function = new OddsCollector.Functions.Functions.EventResultsFunction(loggerMock, processorStub);

        var cancellationToken = new CancellationToken();

        // Act
        var actualEventResults = await function.Run(cancellationToken);

        // Assert
        actualEventResults.Should().NotBeNull().And.BeEmpty();

        loggerMock.Collector.Count.Should().Be(1);
        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Warning);
        loggerMock.LatestRecord.Message.Should().Be("No events received");
    }
}
