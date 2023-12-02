using OddsCollector.Functions.Models;

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
        const string winner = nameof(winner);
        const string id = nameof(id);
        var commenceTime = DateTime.UtcNow;
        var timestamp = DateTime.UtcNow;
        var traceId = Guid.NewGuid();

        var result = new EventResultBuilder()
            .SetWinner(winner)
            .SetId(id)
            .SetCommenceTime(commenceTime)
            .SetTimestamp(timestamp)
            .SetTraceId(traceId)
            .Instance;

        result.Should().NotBeNull();
        result.Winner.Should().NotBeNull().And.Be(winner);
        result.Id.Should().NotBeNull().And.Be(id);
        result.CommenceTime.Should().Be(commenceTime);
        result.Timestamp.Should().Be(timestamp);
        result.TraceId.Should().Be(traceId);
    }

    [Test]
    public void SetWinner_WithValidWinner_ReturnsValidInstance()
    {
        const string winner = nameof(winner);

        var result = new EventResultBuilder().SetWinner(winner).Instance;

        result.Should().NotBeNull();
        result.Winner.Should().NotBeNull().And.Be(winner);
    }

    [Test]
    public void SetId_WithValidId_ReturnsValidInstance()
    {
        const string id = nameof(id);

        var result = new EventResultBuilder().SetId(id).Instance;

        result.Should().NotBeNull();
        result.Id.Should().NotBeNull().And.Be(id);
    }

    [Test]
    public void SetCommenceTime_WithValidCommenceTime_ReturnsValidInstance()
    {
        var commenceTime = DateTime.Now;

        var result = new EventResultBuilder().SetCommenceTime(commenceTime).Instance;

        result.Should().NotBeNull();
        result.CommenceTime.Should().Be(commenceTime);
    }

    [Test]
    public void SetTimestamp_WithValidTimestamp_ReturnsValidInstance()
    {
        var timestamp = DateTime.Now;

        var result = new EventResultBuilder().SetTimestamp(timestamp).Instance;

        result.Should().NotBeNull();
        result.Timestamp.Should().Be(timestamp);
    }

    [Test]
    public void SetTraceId_WithValidTraceId_ReturnsValidInstance()
    {
        var traceId = Guid.NewGuid();

        var result = new EventResultBuilder().SetTraceId(traceId).Instance;

        result.Should().NotBeNull();
        result.TraceId.Should().Be(traceId);
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
