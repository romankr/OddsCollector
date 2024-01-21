﻿using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.Functions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Tests.Tests.Functions;

[Parallelizable(ParallelScope.All)]
internal class EventResultsFunctionTests
{
    [Test]
    public async Task Run_WithValidParameters_ReturnsEventResults()
    {
        IEnumerable<EventResult> expectedEventResults = new List<EventResult>() { new() };

        var loggerMock = Substitute.For<ILogger<EventResultsFunction>>();

        var clientStub = Substitute.For<IOddsApiClient>();
        clientStub.GetEventResultsAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedEventResults));

        var function = new EventResultsFunction(loggerMock, clientStub);

        var token = new CancellationToken();

        var eventResults = await function.Run(token);

        eventResults.Should().NotBeNull().And.BeEquivalentTo(expectedEventResults);

        loggerMock.ReceivedWithAnyArgs().LogInformation(string.Empty, 1);
    }

    [Test]
    public async Task Run_WithException_ReturnsEmptyEventResults()
    {
        var exception = new Exception();

        var loggerMock = Substitute.For<ILogger<EventResultsFunction>>();

        var clientStub = Substitute.For<IOddsApiClient>();
        clientStub.GetEventResultsAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Throws(exception);

        var function = new EventResultsFunction(loggerMock, clientStub);

        var token = new CancellationToken();

        var eventResults = await function.Run(token);

        eventResults.Should().NotBeNull().And.BeEmpty();

        loggerMock.Received().LogError(exception, "Failed to get event results");
    }

    [Test]
    public async Task Run_WithValidParameters_ReturnsNoEventResults()
    {
        IEnumerable<EventResult> expectedEventResults = new List<EventResult>();

        var loggerMock = Substitute.For<ILogger<EventResultsFunction>>();

        var clientStub = Substitute.For<IOddsApiClient>();
        clientStub.GetEventResultsAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedEventResults));

        var function = new EventResultsFunction(loggerMock, clientStub);

        var token = new CancellationToken();

        var eventResults = await function.Run(token);

        eventResults.Should().NotBeNull().And.BeEquivalentTo(expectedEventResults);

        loggerMock.ReceivedWithAnyArgs().LogWarning(string.Empty, 1);
    }
}
