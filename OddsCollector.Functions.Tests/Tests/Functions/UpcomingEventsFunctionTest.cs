using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.Functions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Tests.Tests.Functions;

[Parallelizable(ParallelScope.All)]
internal class UpcomingEventsFunctionTest
{
    [Test]
    public async Task Run_WithValidParameters_ReturnsEventResults()
    {
        IEnumerable<UpcomingEvent> expectedEventResults = new List<UpcomingEvent>();

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
