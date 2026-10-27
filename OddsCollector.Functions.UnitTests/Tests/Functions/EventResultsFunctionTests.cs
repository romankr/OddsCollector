using FluentAssertions.Execution;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.Functions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;
using OddsCollector.Functions.UnitTests.Infrastructure.Models;
using FunctionsRetryAttribute = Microsoft.Azure.Functions.Worker.RetryAttribute;

namespace OddsCollector.Functions.UnitTests.Tests.Functions;

internal sealed class EventResultsFunctionTests
{
    [Test]
    public async Task Run_WithEventResults_ReturnsThemWithoutLogging()
    {
        // Arrange
        EventResult[] eventResults = [ValidModels.CreateEventResult()];

        var clientStub = Substitute.For<IEventResultsClient>();
        clientStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Returns(eventResults);

        var loggerMock = new FakeLogger<EventResultsFunction>();

        var function = new EventResultsFunction(loggerMock, clientStub);

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
        clientStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Returns([]);

        var loggerMock = new FakeLogger<EventResultsFunction>();

        var function = new EventResultsFunction(loggerMock, clientStub);

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
        clientStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Returns([]);

        var loggerMock = new FakeLogger<EventResultsFunction>();

        var function = new EventResultsFunction(loggerMock, clientStub);

        // Act
        var results = await function.Run(new TimerInfo(), new CancellationToken(canceled: true));

        // Assert: the client logs the cancellation, an empty run is not worth a warning then.
        using var scope = new AssertionScope();

        results.Should().BeEmpty();
        loggerMock.Collector.GetSnapshot().Should().BeEmpty();
    }

    [Test]
    public async Task Run_WithHostCancellation_ReturnsNothingAndLogsInformation()
    {
        // Arrange
        var clientStub = Substitute.For<IEventResultsClient>();
        clientStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Throws(new OperationCanceledException());

        var loggerMock = new FakeLogger<EventResultsFunction>();

        var function = new EventResultsFunction(loggerMock, clientStub);

        // Act
        var results = await function.Run(new TimerInfo(), new CancellationToken(canceled: true));

        // Assert
        using var scope = new AssertionScope();

        results.Should().BeEmpty();
        loggerMock.Collector.GetSnapshot().Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { Level = LogLevel.Information, Message = "Collection was cancelled" });
    }

    [Test]
    public async Task Run_WithCancellationNotRequestedByHost_LetsItReachTheHost()
    {
        // Arrange: e.g. an HTTP timeout surfaces as OperationCanceledException without the host cancelling.
        var expectedException = new OperationCanceledException();

        var clientStub = Substitute.For<IEventResultsClient>();
        clientStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Throws(expectedException);

        var loggerMock = new FakeLogger<EventResultsFunction>();

        var function = new EventResultsFunction(loggerMock, clientStub);

        // Act
        var action = () => function.Run(new TimerInfo(), CancellationToken.None);

        // Assert
        (await action.Should().ThrowAsync<OperationCanceledException>()).Which.Should().BeSameAs(expectedException);
        loggerMock.Collector.GetSnapshot().Should().BeEmpty();
    }

    [Test]
    public async Task Run_WithClientException_LetsItReachTheHost()
    {
        // Arrange
        var expectedException = new InvalidOperationException();

        var clientStub = Substitute.For<IEventResultsClient>();
        clientStub.GetEventResultsAsync(Arg.Any<CancellationToken>()).Throws(expectedException);

        var loggerMock = new FakeLogger<EventResultsFunction>();

        var function = new EventResultsFunction(loggerMock, clientStub);

        // Act
        var action = () => function.Run(new TimerInfo(), CancellationToken.None);

        // Assert
        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(expectedException);
        loggerMock.Collector.GetSnapshot().Should().BeEmpty();
    }

    [Test]
    public void Run_FailedRun_IsNotRetriedByTheHost()
    {
        // Arrange
        var method = typeof(EventResultsFunction).GetMethod(nameof(EventResultsFunction.Run))!;

        // Act
        var retries = method.GetCustomAttributes(typeof(FunctionsRetryAttribute), inherit: false);

        // Assert: failed Odds API requests are retried by the HTTP client; retrying the whole run on top of that
        // would repeat every league's request.
        retries.Should().BeEmpty();
    }
}
