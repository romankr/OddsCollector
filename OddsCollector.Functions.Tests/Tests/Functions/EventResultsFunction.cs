using FluentAssertions.Execution;
using Microsoft.Azure.Functions.Worker;
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
        var results = await function.Run(new TimerInfo(), CancellationToken.None);

        // Assert
        results.Should().NotBeNull().And.BeEquivalentTo(expectedResults);

        loggerMock.Collector.Count.Should().Be(0);
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
        var results = await function.Run(new TimerInfo(), CancellationToken.None);

        // Assert
        results.Should().NotBeNull().And.BeEmpty();

        loggerMock.Collector.Count.Should().Be(1);

        using var scope = new AssertionScope();

        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Warning);
        loggerMock.LatestRecord.Message.Should().Be("No events received");
    }

    [Test]
    public async Task Run_WithCancellation_ReturnsNothingAndLogsInformation()
    {
        // Arrange
        var loggerMock = new FakeLogger<FunctionApp.EventResultsFunction>();

        var clientStub = Substitute.For<IEventResultsClient>();
        clientStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Throws(new OperationCanceledException());

        var function = new FunctionApp.EventResultsFunction(loggerMock, clientStub);

        // Act
        var results = await function.Run(new TimerInfo(), CancellationToken.None);

        // Assert
        results.Should().NotBeNull().And.BeEmpty();

        loggerMock.Collector.Count.Should().Be(1);

        using var scope = new AssertionScope();

        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Information);
        loggerMock.LatestRecord.Message.Should().Be("Collection was cancelled");
    }

    [Test]
    public async Task Run_WithException_Throws()
    {
        // Arrange
        var exception = new InvalidOperationException();

        var loggerMock = new FakeLogger<FunctionApp.EventResultsFunction>();

        var clientStub = Substitute.For<IEventResultsClient>();
        clientStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Throws(exception);

        var function = new FunctionApp.EventResultsFunction(loggerMock, clientStub);

        // Act
        var action = () => function.Run(new TimerInfo(), CancellationToken.None);

        // Assert
        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Should().Be(exception);

        loggerMock.Collector.Count.Should().Be(0);
    }
}
