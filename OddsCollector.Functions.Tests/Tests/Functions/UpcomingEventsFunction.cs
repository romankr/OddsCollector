﻿using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Processors;
using FunctionApp = OddsCollector.Functions.Functions;

namespace OddsCollector.Functions.Tests.Tests.Functions;

internal sealed class UpcomingEventsFunction
{
    [Test]
    public async Task Run_WithValidMessages_ReturnsEventResultList()
    {
        // Arrange
        UpcomingEvent[] expectedEventResults = [new()];

        var loggerStub = new FakeLogger<FunctionApp.UpcomingEventsFunction>();

        var processorStub = Substitute.For<IUpcomingEventsProcessor>();

        processorStub.GetUpcomingEventsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedEventResults));

        var function = new FunctionApp.UpcomingEventsFunction(loggerStub, processorStub);

        // Act
        var eventResults = await function.Run(CancellationToken.None);

        // Assert
        eventResults.Should().NotBeNull().And.BeEquivalentTo(expectedEventResults);
    }

    [Test]
    public async Task Run_WithException_ReturnsEmptyEventResultListAndLogsException()
    {
        // Arrange
        var exception = new Exception();

        var loggerMock = new FakeLogger<FunctionApp.UpcomingEventsFunction>();

        var processorStub = Substitute.For<IUpcomingEventsProcessor>();

        processorStub.GetUpcomingEventsAsync(Arg.Any<CancellationToken>()).Throws(exception);

        var function = new FunctionApp.UpcomingEventsFunction(loggerMock, processorStub);

        // Act
        var eventResults = await function.Run(CancellationToken.None);

        // Assert
        eventResults.Should().NotBeNull().And.BeEmpty();

        loggerMock.Collector.Count.Should().Be(1);

        using var scope = new AssertionScope();

        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Error);
        loggerMock.LatestRecord.Message.Should().Be("Failed to get events");
        loggerMock.LatestRecord.Exception.Should().Be(exception);
    }
}
