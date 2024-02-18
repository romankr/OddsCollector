﻿using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.Functions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Tests.Tests.Functions;

internal class UpcomingEventsFunctionTest
{
    [Test]
    public async Task Run_WithValidParameters_ReturnsEventResults()
    {
        // Arrange
        IEnumerable<UpcomingEvent> expectedEventResults = new List<UpcomingEvent>() { new() };

        var loggerMock = Substitute.For<ILogger<UpcomingEventsFunction>>();

        var clientStub = Substitute.For<IOddsApiClient>();
        clientStub.GetUpcomingEventsAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedEventResults));

        var function = new UpcomingEventsFunction(loggerMock, clientStub);

        // Act
        var eventResults = await function.Run(new CancellationToken());

        // Assert
        eventResults.Should().NotBeNull().And.BeEquivalentTo(expectedEventResults);

        loggerMock.ReceivedWithAnyArgs().LogInformation(string.Empty, 1);
    }

    [Test]
    public async Task Run_WithException_ReturnsEmptyEventResults()
    {
        // Arrange
        var exception = new Exception();

        var loggerMock = Substitute.For<ILogger<UpcomingEventsFunction>>();

        var clientStub = Substitute.For<IOddsApiClient>();
        clientStub.GetUpcomingEventsAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Throws(exception);

        var function = new UpcomingEventsFunction(loggerMock, clientStub);

        // Act
        var eventResults = await function.Run(new CancellationToken());

        // Assert
        eventResults.Should().NotBeNull().And.BeEmpty();

        loggerMock.Received().LogError(exception, "Failed to get upcoming events");
    }

    [Test]
    public async Task Run_WithValidParameters_ReturnsNoEventResults()
    {
        // Arrange
        IEnumerable<UpcomingEvent> expectedEventResults = new List<UpcomingEvent>();

        var loggerMock = Substitute.For<ILogger<UpcomingEventsFunction>>();

        var clientStub = Substitute.For<IOddsApiClient>();
        clientStub.GetUpcomingEventsAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedEventResults));

        var function = new UpcomingEventsFunction(loggerMock, clientStub);

        // Act
        var eventResults = await function.Run(new CancellationToken());

        // Assert
        eventResults.Should().NotBeNull().And.BeEquivalentTo(expectedEventResults);

        loggerMock.ReceivedWithAnyArgs().LogWarning(string.Empty, 1);
    }
}
