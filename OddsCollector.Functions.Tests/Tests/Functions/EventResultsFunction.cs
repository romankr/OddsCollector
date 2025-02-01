using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework.Internal;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Processors;

namespace OddsCollector.Functions.Tests.Tests.Functions;

internal sealed class EventResultsFunction
{
    [Test]
    public async Task Run_WithValidMessages_ReturnsEventResultList()
    {
        // Arrange
        EventResult[] expectedEventResults = [new()];

        var loggerStub = new FakeLogger<OddsCollector.Functions.Functions.EventResultsFunction>();

        var processorStub = Substitute.For<IEventResultProcessor>();

        processorStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedEventResults));

        var function = new OddsCollector.Functions.Functions.EventResultsFunction(loggerStub, processorStub);

        // Act
        var actualEventResults = await function.Run(new CancellationToken());

        // Assert
        actualEventResults.Should().NotBeNull().And.BeEquivalentTo(expectedEventResults);
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

        using var scope = new AssertionScope();

        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Error);
        loggerMock.LatestRecord.Message.Should().Be("Failed to get events");
        loggerMock.LatestRecord.Exception.Should().Be(exception);
    }
}
