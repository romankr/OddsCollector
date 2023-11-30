using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Common.Models;
using OddsCollector.Common.OddsApi;

namespace OddsCollector.Functions.UpcomingEvents.Tests;

[Parallelizable(ParallelScope.All)]
internal class UpcomingEventsFunctionTest
{
    [Test]
    public void Constructor_WithValidDependencies_ReturnsNewInstance()
    {
        var loggerStub = Substitute.For<ILogger<UpcomingEventsFunction>>();
        var clientStub = Substitute.For<IOddsApiClient>();

        var function = new UpcomingEventsFunction(loggerStub, clientStub);

        function.Should().NotBeNull();
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsException()
    {
        var clientStub = Substitute.For<IOddsApiClient>();

        var action = () =>
        {
            _ = new UpcomingEventsFunction(null, clientStub);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void Constructor_WithNullOddsClient_ThrowsException()
    {
        var loggerStub = Substitute.For<ILogger<UpcomingEventsFunction>>();

        var action = () =>
        {
            _ = new UpcomingEventsFunction(loggerStub, null);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Test]
    public async Task Run_WithValidParameters_ReturnsEventResults()
    {
        IEnumerable<UpcomingEvent> expectedEventResults = [];

        var loggerStub = Substitute.For<ILogger<UpcomingEventsFunction>>();

        var clientStub = Substitute.For<IOddsApiClient>();
        clientStub.GetUpcomingEventsAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedEventResults));

        var function = new UpcomingEventsFunction(loggerStub, clientStub);

        var eventResults = await function.Run(new CancellationToken());

        eventResults.Should().NotBeNull().And.BeEquivalentTo(expectedEventResults);
    }

    [Test]
    public async Task Run_WithException_ReturnsEmptyEventResults()
    {
        var exception = new Exception();

        var loggerMock = Substitute.For<ILogger<UpcomingEventsFunction>>();

        var clientStub = Substitute.For<IOddsApiClient>();
        clientStub.GetUpcomingEventsAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Throws(exception);

        var function = new UpcomingEventsFunction(loggerMock, clientStub);

        var eventResults = await function.Run(new CancellationToken());

        eventResults.Should().NotBeNull().And.BeEmpty();

        loggerMock.Received().LogError(exception, "Failed to get upcoming events");
    }
}
