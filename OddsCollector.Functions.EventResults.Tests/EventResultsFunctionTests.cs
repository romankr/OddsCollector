using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using NSubstitute;
using OddsCollector.Common.Models;
using OddsCollector.Common.OddsApi.Client;

namespace OddsCollector.Functions.EventResults.Tests;

internal sealed class EventResultsFunctionTests
{
    [Test]
    public void Constructor_WithValidDependencies_ReturnsNewInstance()
    {
        var clientStub = Substitute.For<IOddsApiClient>();

        var function = new EventResultsFunction(clientStub);

        function.Should().NotBeNull();
    }

    [Test]
    public void Constructor_WithNullOddsClient_ThrowsException()
    {
        var action = () =>
        {
            _ = new EventResultsFunction(null);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Test]
    public async Task Run_WithValidTimer_ReturnsEventResults()
    {
        IEnumerable<EventResult> expectedEventResults = [];

        var clientStub = Substitute.For<IOddsApiClient>();
        clientStub.GetEventResultsAsync(Arg.Any<Guid>(), Arg.Any<DateTime>())
            .Returns(Task.FromResult(expectedEventResults));

        var function = new EventResultsFunction(clientStub);

        var timerStub = Substitute.For<TimerInfo>();

        var eventResults = await function.Run(timerStub);

        eventResults.Should().NotBeNull().And.BeEquivalentTo(expectedEventResults);
    }
}
