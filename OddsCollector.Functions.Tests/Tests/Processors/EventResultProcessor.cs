﻿using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;
using FunctionApp = OddsCollector.Functions.Processors;

namespace OddsCollector.Functions.Tests.Tests.Processors;

internal sealed class EventResultProcessor
{
    [Test]
    public async Task GetEventResultsAsync_WithNoEvents_ReturnsNoEventAngLogsWarning()
    {
        // Arrange
        EventResult[] expectedEventResults = [];

        var loggerMock = new FakeLogger<FunctionApp.EventResultProcessor>();

        var clientMock = Substitute.For<IEventResultsClient>();
        clientMock.GetEventResultsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedEventResults));

        var processor = new FunctionApp.EventResultProcessor(loggerMock, clientMock);

        // Act
        var actualEventResults = await processor.GetEventResultsAsync(CancellationToken.None);

        // Assert
        actualEventResults.Should().NotBeNull().And.BeEmpty();

        loggerMock.Collector.Count.Should().Be(1);

        using var scope = new AssertionScope();

        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Warning);
        loggerMock.LatestRecord.Message.Should().Be("No events received");
    }

    [Test]
    public async Task GetEventResultsAsync_WithSingleEvent_ReturnsSingleEventAngLogsInformation()
    {
        // Arrange
        var expectedEventResult = new EventResult();

        EventResult[] expectedEventResults = [expectedEventResult];

        var loggerMock = new FakeLogger<FunctionApp.EventResultProcessor>();

        var clientMock = Substitute.For<IEventResultsClient>();
        clientMock.GetEventResultsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedEventResults));

        var processor = new FunctionApp.EventResultProcessor(loggerMock, clientMock);

        // Act
        var actualEventResults = await processor.GetEventResultsAsync(CancellationToken.None);

        // Assert
        actualEventResults.Should().NotBeNull().And.HaveCount(1);
        actualEventResults[0].Should().NotBeNull().And.Be(expectedEventResult);

        loggerMock.Collector.Count.Should().Be(1);

        using var scope = new AssertionScope();

        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Information);
        loggerMock.LatestRecord.Message.Should().Be("1 event(s) received");
    }
}
