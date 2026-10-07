using FluentAssertions.Execution;
using Microsoft.Azure.Functions.Worker;
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
    public async Task Run_WithUpcomingEvents_ReturnsThemWithoutLogging()
    {
        // Arrange
        UpcomingEvent[] upcomingEvents = [new()];

        var clientStub = Substitute.For<IUpcomingEventsClient>();
        clientStub.GetUpcomingEventsAsync(Arg.Any<CancellationToken>()).Returns(upcomingEvents);

        var loggerMock = new FakeLogger<FunctionApp.UpcomingEventsFunction>();

        var function = new FunctionApp.UpcomingEventsFunction(loggerMock, clientStub);

        // Act
        var results = await function.Run(new TimerInfo(), CancellationToken.None);

        // Assert
        using var scope = new AssertionScope();

        results.Should().Equal(upcomingEvents);
        loggerMock.Collector.GetSnapshot().Should().BeEmpty();
    }

    [Test]
    public async Task Run_WithNoUpcomingEvents_ReturnsNothingAndLogsWarning()
    {
        // Arrange
        var clientStub = Substitute.For<IUpcomingEventsClient>();
        clientStub.GetUpcomingEventsAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<UpcomingEvent>());

        var loggerMock = new FakeLogger<FunctionApp.UpcomingEventsFunction>();

        var function = new FunctionApp.UpcomingEventsFunction(loggerMock, clientStub);

        // Act
        var results = await function.Run(new TimerInfo(), CancellationToken.None);

        // Assert
        using var scope = new AssertionScope();

        results.Should().BeEmpty();
        loggerMock.Collector.GetSnapshot().Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { Level = LogLevel.Warning, Message = "No events received" });
    }

    [Test]
    public async Task Run_WithCancellation_ReturnsNothingAndLogsInformation()
    {
        // Arrange
        var clientStub = Substitute.For<IUpcomingEventsClient>();
        clientStub.GetUpcomingEventsAsync(Arg.Any<CancellationToken>()).Throws(new OperationCanceledException());

        var loggerMock = new FakeLogger<FunctionApp.UpcomingEventsFunction>();

        var function = new FunctionApp.UpcomingEventsFunction(loggerMock, clientStub);

        // Act
        var results = await function.Run(new TimerInfo(), CancellationToken.None);

        // Assert: the host winding a run down is not a failure to report.
        using var scope = new AssertionScope();

        results.Should().BeEmpty();
        loggerMock.Collector.GetSnapshot().Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { Level = LogLevel.Information, Message = "Collection was cancelled" });
    }

    [Test]
    public async Task Run_WithClientException_LetsItReachTheHost()
    {
        // Arrange
        var expectedException = new InvalidOperationException();

        var clientStub = Substitute.For<IUpcomingEventsClient>();
        clientStub.GetUpcomingEventsAsync(Arg.Any<CancellationToken>()).Throws(expectedException);

        var loggerMock = new FakeLogger<FunctionApp.UpcomingEventsFunction>();

        var function = new FunctionApp.UpcomingEventsFunction(loggerMock, clientStub);

        // Act
        var action = () => function.Run(new TimerInfo(), CancellationToken.None);

        // Assert: the host logs the failure and marks the run failed.
        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(expectedException);
        loggerMock.Collector.GetSnapshot().Should().BeEmpty();
    }
}
