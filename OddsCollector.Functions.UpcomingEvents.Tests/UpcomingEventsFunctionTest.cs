﻿using FluentAssertions;
using NSubstitute;
using OddsCollector.Common.Models;
using OddsCollector.Common.OddsApi.Client;

namespace OddsCollector.Functions.UpcomingEvents.Tests;

[Parallelizable(ParallelScope.All)]
internal class UpcomingEventsFunctionTest
{
    [Test]
    public void Constructor_WithValidDependencies_ReturnsNewInstance()
    {
        var clientStub = Substitute.For<IOddsApiClient>();

        var function = new UpcomingEventsFunction(clientStub);

        function.Should().NotBeNull();
    }

    [Test]
    public void Constructor_WithNullOddsClient_ThrowsException()
    {
        var action = () =>
        {
            _ = new UpcomingEventsFunction(null);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Test]
    public async Task Run_WithValidTimer_ReturnsEventResults()
    {
        IEnumerable<UpcomingEvent> expectedEventResults = [];

        var clientStub = Substitute.For<IOddsApiClient>();
        clientStub.GetUpcomingEventsAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedEventResults));

        var function = new UpcomingEventsFunction(clientStub);

        var eventResults = await function.Run(new CancellationToken());

        eventResults.Should().NotBeNull().And.BeEquivalentTo(expectedEventResults);
    }
}
