using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;
using FunctionApp = OddsCollector.Functions.Functions;

namespace OddsCollector.Functions.Tests.Tests.Functions;

internal sealed class UpcomingEventsFunction
{
    [Test]
    public async Task Run_WithUpcomingEvents_ReturnsUpcomingEventsAndLogsInformation()
    {
        // Arrange
        var expected = new UpcomingEvent();
        UpcomingEvent[] expectedResults = [expected];

        var loggerMock = new FakeLogger<FunctionApp.UpcomingEventsFunction>();

        var clientStub = Substitute.For<IUpcomingEventsClient>();
        clientStub.GetUpcomingEventsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResults));

        var function = new FunctionApp.UpcomingEventsFunction(loggerMock, clientStub);

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
    public async Task Run_WithNoUpcomingEvents_ReturnsNoUpcomingEventsAndLogsWarning()
    {
        // Arrange
        UpcomingEvent[] expectedResults = [];

        var loggerMock = new FakeLogger<FunctionApp.UpcomingEventsFunction>();

        var clientStub = Substitute.For<IUpcomingEventsClient>();
        clientStub.GetUpcomingEventsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResults));

        var function = new FunctionApp.UpcomingEventsFunction(loggerMock, clientStub);

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
    public async Task Run_WithException_ReturnsEmptyUpcomingEventListAndLogsException()
    {
        // Arrange
        var exception = new Exception();

        var loggerMock = new FakeLogger<FunctionApp.UpcomingEventsFunction>();

        var clientStub = Substitute.For<IUpcomingEventsClient>();
        clientStub.GetUpcomingEventsAsync(Arg.Any<CancellationToken>()).Throws(exception);

        var function = new FunctionApp.UpcomingEventsFunction(loggerMock, clientStub);

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
