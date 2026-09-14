using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;
using FunctionApp = OddsCollector.Functions.Functions;

namespace OddsCollector.Functions.Tests.Tests.Functions;

internal sealed class EventResultsFunction
{
    [Test]
    public async Task Run_WithEventResults_ReturnsEventResultsAndLogsInformation()
    {
        // Arrange
        var expected = new EventResult();
        EventResult[] expectedResults = [expected];

        var loggerMock = new FakeLogger<FunctionApp.EventResultsFunction>();

        var clientStub = Substitute.For<IEventResultsClient>();
        clientStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResults));

        var function = new FunctionApp.EventResultsFunction(loggerMock, clientStub);

        // Act
        var results = await function.Run(CancellationToken.None);

        // Assert
        results.Should().NotBeNull().And.BeEquivalentTo(expectedResults);

        loggerMock.Collector.Count.Should().Be(1);

        using var scope = new AssertionScope();

        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Information);
        loggerMock.LatestRecord.Message.Should().Be("1 event(s) received");
    }

    [Test]
    public async Task Run_WithNoEventResults_ReturnsNoEventResultsAndLogsWarning()
    {
        // Arrange
        EventResult[] expectedResults = [];

        var loggerMock = new FakeLogger<FunctionApp.EventResultsFunction>();

        var clientStub = Substitute.For<IEventResultsClient>();
        clientStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResults));

        var function = new FunctionApp.EventResultsFunction(loggerMock, clientStub);

        // Act
        var results = await function.Run(CancellationToken.None);

        // Assert
        results.Should().NotBeNull().And.BeEmpty();

        loggerMock.Collector.Count.Should().Be(1);

        using var scope = new AssertionScope();

        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Warning);
        loggerMock.LatestRecord.Message.Should().Be("No events received");
    }

    [Test]
    public async Task Run_WithException_ReturnsEmptyEventResultListAndLogsException()
    {
        // Arrange
        var exception = new Exception();

        var loggerMock = new FakeLogger<FunctionApp.EventResultsFunction>();

        var clientStub = Substitute.For<IEventResultsClient>();
        clientStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Throws(exception);

        var function = new FunctionApp.EventResultsFunction(loggerMock, clientStub);

        // Act
        var results = await function.Run(CancellationToken.None);

        // Assert
        results.Should().NotBeNull().And.BeEmpty();

        loggerMock.Collector.Count.Should().Be(1);

        using var scope = new AssertionScope();

        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Error);
        loggerMock.LatestRecord.Message.Should().Be("Failed to get events");
        loggerMock.LatestRecord.Exception.Should().Be(exception);
    }
}
