using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.Functions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Tests.Tests.Functions;

[Parallelizable(ParallelScope.All)]
internal sealed class EventResultsFunctionTests
{
    [Test]
    public void Constructor_WithValidDependencies_ReturnsNewInstance()
    {
        var loggerStub = Substitute.For<ILogger<EventResultsFunction>>();
        var clientStub = Substitute.For<IOddsApiClient>();

        var function = new EventResultsFunction(loggerStub, clientStub);

        function.Should().NotBeNull();
    }

    [Test]
    public void Constructor_WithNullOddsClient_ThrowsException()
    {
        var loggerStub = Substitute.For<ILogger<EventResultsFunction>>();

        var action = () =>
        {
            _ = new EventResultsFunction(loggerStub, null);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsException()
    {
        var clientStub = Substitute.For<IOddsApiClient>();

        var action = () =>
        {
            _ = new EventResultsFunction(null, clientStub);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public async Task Run_WithValidParameters_ReturnsEventResults()
    {
        IEnumerable<EventResult> expectedEventResults = new List<EventResult>();

        var loggerStub = Substitute.For<ILogger<EventResultsFunction>>();

        var clientStub = Substitute.For<IOddsApiClient>();
        clientStub.GetEventResultsAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedEventResults));

        var function = new EventResultsFunction(loggerStub, clientStub);

        var token = new CancellationToken();

        var eventResults = await function.Run(token);

        eventResults.Should().NotBeNull().And.BeEquivalentTo(expectedEventResults);
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
}
