using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;
using LoggerFactory = OddsCollector.Functions.Tests.Infrastructure.Logger.LoggerFactory;

namespace OddsCollector.Functions.Tests.Tests.Functions;

internal class EventResultsFunction
{
    [Test]
    [SuppressMessage("Usage", "CA2254:Template should be a static expression")]
    public async Task Run_WithValidParameters_ReturnsEventResults()
    {
        // Arrange
        IEnumerable<EventResult> expectedEventResults = new List<EventResult> { new() };

        var loggerMock = LoggerFactory.GetLoggerMock<OddsCollector.Functions.Functions.EventResultsFunction>();

        var clientStub = GetOddsApiClientStub(expectedEventResults);

        var function = new OddsCollector.Functions.Functions.EventResultsFunction(loggerMock, clientStub);

        var cancellationToken = new CancellationToken();

        // Act
        var eventResults = await function.Run(cancellationToken);

        // Assert
        eventResults.Should().NotBeNull().And.BeEquivalentTo(expectedEventResults);

        loggerMock.ReceivedWithAnyArgs().LogInformation(string.Empty, 1);
    }

    [Test]
    public async Task Run_WithException_ReturnsEmptyEventResults()
    {
        // Arrange
        var exception = new Exception();

        var loggerMock = LoggerFactory.GetLoggerMock<OddsCollector.Functions.Functions.EventResultsFunction>();

        var clientStub = Substitute.For<IOddsApiClient>();
        clientStub.GetEventResultsAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Throws(exception);

        var function = new OddsCollector.Functions.Functions.EventResultsFunction(loggerMock, clientStub);

        var cancellationToken = new CancellationToken();

        // Act
        var eventResults = await function.Run(cancellationToken);

        // Assert
        eventResults.Should().NotBeNull().And.BeEmpty();

        loggerMock.Received().LogError(exception, "Failed to get event results");
    }

    [Test]
    [SuppressMessage("Usage", "CA2254:Template should be a static expression")]
    public async Task Run_WithValidParameters_ReturnsNoEventResults()
    {
        // Arrange
        IEnumerable<EventResult> expectedEventResults = new List<EventResult>();

        var loggerMock = LoggerFactory.GetLoggerMock<OddsCollector.Functions.Functions.EventResultsFunction>();

        var clientStub = GetOddsApiClientStub(expectedEventResults);

        var function = new OddsCollector.Functions.Functions.EventResultsFunction(loggerMock, clientStub);

        var cancellationToken = new CancellationToken();

        // Act
        var eventResults = await function.Run(cancellationToken);

        // Assert
        eventResults.Should().NotBeNull().And.BeEquivalentTo(expectedEventResults);

        loggerMock.ReceivedWithAnyArgs().LogWarning(string.Empty, 1);
    }

    private static IOddsApiClient GetOddsApiClientStub(IEnumerable<EventResult> expectedEventResults)
    {
        var clientStub = Substitute.For<IOddsApiClient>();

        clientStub.GetEventResultsAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedEventResults));

        return clientStub;
    }
}
