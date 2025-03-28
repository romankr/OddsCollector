﻿using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;
using FunctionApp = OddsCollector.Functions.Processors;

namespace OddsCollector.Functions.Tests.Tests.Processors;

internal sealed class UpcomingEventsProcessor
{
    [Test]
    public async Task GetUpcomingEventsAsync_WithNoEvents_ReturnsNoEventAngLogsWarning()
    {
        // Arrange
        UpcomingEvent[] expectedUpcomingEvents = [];

        var loggerMock = new FakeLogger<FunctionApp.UpcomingEventsProcessor>();

        var clientMock = Substitute.For<IUpcomingEventsClient>();
        clientMock.GetUpcomingEventsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedUpcomingEvents));

        var processor = new FunctionApp.UpcomingEventsProcessor(loggerMock, clientMock);

        // Act
        var actualEventResults = await processor.GetUpcomingEventsAsync(CancellationToken.None);

        // Assert
        actualEventResults.Should().NotBeNull().And.BeEmpty();

        loggerMock.Collector.Count.Should().Be(1);

        using var scope = new AssertionScope();

        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Warning);
        loggerMock.LatestRecord.Message.Should().Be("No events received");
    }

    [Test]
    public async Task GetUpcomingEventsAsync_WithSingleEvent_ReturnsSingleEventAngLogsInformation()
    {
        // Arrange
        var expectedUpcomingEvent = new UpcomingEvent();

        UpcomingEvent[] expectedUpcomingEvents = [expectedUpcomingEvent];

        var loggerMock = new FakeLogger<FunctionApp.UpcomingEventsProcessor>();

        var clientMock = Substitute.For<IUpcomingEventsClient>();
        clientMock.GetUpcomingEventsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedUpcomingEvents));

        var processor = new FunctionApp.UpcomingEventsProcessor(loggerMock, clientMock);

        // Act
        var actualEventResults = await processor.GetUpcomingEventsAsync(CancellationToken.None);

        // Assert
        actualEventResults.Should().NotBeNull().And.HaveCount(1);
        actualEventResults[0].Should().NotBeNull().And.Be(expectedUpcomingEvent);

        loggerMock.Collector.Count.Should().Be(1);

        using var scope = new AssertionScope();

        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Information);
        loggerMock.LatestRecord.Message.Should().Be("1 event(s) received");
    }
}
