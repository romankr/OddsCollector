using OddsCollector.Functions.Models;
using OddsCollector.Functions.Tests.Data;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Models;

[Parallelizable(ParallelScope.All)]
internal class EventResultBuilderTests
{
    [Test]
    public void Instance_WithoutParameters_ReturnsValidInstance()
    {
        var result = new EventResultBuilder().Instance;

        result.Should().NotBeNull();
    }

    [Test]
    public void Instance_WithFullParameterList_ReturnsValidInstance()
    {
        var result = new EventResultBuilder()
            .SetWinner(SampleEvent.Winner)
            .SetId(SampleEvent.Id)
            .SetCommenceTime(SampleEvent.CommenceTime)
            .SetTimestamp(SampleEvent.Timestamp)
            .SetTraceId(SampleEvent.TraceId)
            .Instance;

        result.Should().NotBeNull();
        result.Winner.Should().NotBeNull().And.Be(SampleEvent.Winner);
        result.Id.Should().NotBeNull().And.Be(SampleEvent.Id);
        result.CommenceTime.Should().Be(SampleEvent.CommenceTime);
        result.Timestamp.Should().Be(SampleEvent.Timestamp);
        result.TraceId.Should().Be(SampleEvent.TraceId);
    }

    [Test]
    public void SetWinner_WithValidWinner_ReturnsValidInstance()
    {
        var result = new EventResultBuilder().SetWinner(SampleEvent.Winner).Instance;

        result.Should().NotBeNull();
        result.Winner.Should().NotBeNull().And.Be(SampleEvent.Winner);
    }

    [Test]
    public void SetId_WithValidId_ReturnsValidInstance()
    {
        var result = new EventResultBuilder().SetId(SampleEvent.Id).Instance;

        result.Should().NotBeNull();
        result.Id.Should().NotBeNull().And.Be(SampleEvent.Id);
    }

    [Test]
    public void SetCommenceTime_WithValidCommenceTime_ReturnsValidInstance()
    {
        var result = new EventResultBuilder().SetCommenceTime(SampleEvent.CommenceTime).Instance;

        result.Should().NotBeNull();
        result.CommenceTime.Should().Be(SampleEvent.CommenceTime);
    }

    [Test]
    public void SetTimestamp_WithValidTimestamp_ReturnsValidInstance()
    {
        var result = new EventResultBuilder().SetTimestamp(SampleEvent.Timestamp).Instance;

        result.Should().NotBeNull();
        result.Timestamp.Should().Be(SampleEvent.Timestamp);
    }

    [Test]
    public void SetTraceId_WithValidTraceId_ReturnsValidInstance()
    {
        var result = new EventResultBuilder().SetTraceId(SampleEvent.TraceId).Instance;

        result.Should().NotBeNull();
        result.TraceId.Should().Be(SampleEvent.TraceId);
    }

    [TestCase("")]
    [TestCase(null)]
    public void SetWinner_WithNullOrEmptyWinner_ThrowsException(string? winner)
    {
        var action = () =>
        {
            _ = new EventResultBuilder().SetWinner(winner).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(winner));
    }

    [TestCase("")]
    [TestCase(null)]
    public void SetId_WithNullOrEmptyId_ThrowsException(string? id)
    {
        var action = () =>
        {
            _ = new EventResultBuilder().SetId(id).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(id));
    }

    [Test]
    public void SetCommenceTime_WithNullCommenceTime_ThrowsException()
    {
        var action = () =>
        {
            _ = new EventResultBuilder().SetCommenceTime(null).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName("commenceTime");
    }

    [Test]
    public void SetTimestamp_WithNullTimestamp_ThrowsException()
    {
        var action = () =>
        {
            _ = new EventResultBuilder().SetTimestamp(null).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName("timestamp");
    }

    [Test]
    public void SetTraceId_WithNullTraceId_ThrowsException()
    {
        var action = () =>
        {
            _ = new EventResultBuilder().SetTraceId(null).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName("traceId");
    }
}
