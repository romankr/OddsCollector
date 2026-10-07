using FluentAssertions.Execution;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;
using OddsCollector.Functions.Tests.Infrastructure.Models;
using FunctionApp = OddsCollector.Functions.Functions;

namespace OddsCollector.Functions.Tests.Tests.Functions;

internal sealed class EventResultsFunction
{
    [Test]
    public async Task Run_WithEventResults_ReturnsThemWithoutLogging()
    {
        // Arrange
        EventResult[] eventResults = [ValidModels.CreateEventResult()];

        var clientStub = Substitute.For<IEventResultsClient>();
        clientStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Returns(eventResults);

        var loggerMock = new FakeLogger<FunctionApp.EventResultsFunction>();

        var function = new FunctionApp.EventResultsFunction(loggerMock, clientStub);

        // Act
        var results = await function.Run(new TimerInfo(), CancellationToken.None);

        // Assert
        using var scope = new AssertionScope();

        results.Should().Equal(eventResults);
        loggerMock.Collector.GetSnapshot().Should().BeEmpty();
    }

    [Test]
    public async Task Run_WithNoEventResults_ReturnsNothingAndLogsWarning()
    {
        // Arrange
        var clientStub = Substitute.For<IEventResultsClient>();
        clientStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<EventResult>());

        var loggerMock = new FakeLogger<FunctionApp.EventResultsFunction>();

        var function = new FunctionApp.EventResultsFunction(loggerMock, clientStub);

        // Act
        var results = await function.Run(new TimerInfo(), CancellationToken.None);

        // Assert
        using var scope = new AssertionScope();

        results.Should().BeEmpty();
        loggerMock.Collector.GetSnapshot().Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { Level = LogLevel.Warning, Message = "No events received" });
    }

    [Test]
    public async Task Run_WithNothingCollectedBeforeCancellation_ReturnsNothingWithoutWarning()
    {
        // Arrange
        var clientStub = Substitute.For<IEventResultsClient>();
        clientStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<EventResult>());

        var loggerMock = new FakeLogger<FunctionApp.EventResultsFunction>();

        var function = new FunctionApp.EventResultsFunction(loggerMock, clientStub);

        // Act
        var results = await function.Run(new TimerInfo(), new CancellationToken(canceled: true));

        // Assert: the client logs the cancellation, an empty run is not worth a warning then.
        using var scope = new AssertionScope();

        results.Should().BeEmpty();
        loggerMock.Collector.GetSnapshot().Should().BeEmpty();
    }

    [Test]
    public async Task Run_WithCancellation_ReturnsNothingAndLogsInformation()
    {
        // Arrange
        var clientStub = Substitute.For<IEventResultsClient>();
        clientStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Throws(new OperationCanceledException());

        var loggerMock = new FakeLogger<FunctionApp.EventResultsFunction>();

        var function = new FunctionApp.EventResultsFunction(loggerMock, clientStub);

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

        var clientStub = Substitute.For<IEventResultsClient>();
        clientStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Throws(expectedException);

        var loggerMock = new FakeLogger<FunctionApp.EventResultsFunction>();

        var function = new FunctionApp.EventResultsFunction(loggerMock, clientStub);

        // Act
        var action = () => function.Run(new TimerInfo(), CancellationToken.None);

        // Assert: the host logs the failure and marks the run failed.
        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(expectedException);
        loggerMock.Collector.GetSnapshot().Should().BeEmpty();
    }
}
