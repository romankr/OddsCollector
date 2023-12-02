using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.OddsApi.Models;

[Parallelizable(ParallelScope.All)]
internal class UpcomingEventBuilderTests
{
    [Test]
    public void Constructor_WithoutParameters_ReturnsValidInstance()
    {
        var result = new UpcomingEventBuilder().Instance;

        result.Should().NotBeNull();
    }

    [Test]
    public void Constructor_WithFullParameterList_ReturnsValidInstance()
    {
        const string awayTeam = nameof(awayTeam);
        const string homeTeam = nameof(homeTeam);
        const string id = nameof(id);
        var commenceTime = DateTime.UtcNow;
        var timestamp = DateTime.UtcNow;
        var traceId = Guid.NewGuid();

        var result = new UpcomingEventBuilder()
            .SetAwayTeam(awayTeam)
            .SetHomeTeam(homeTeam)
            .SetId(id)
            .SetCommenceTime(commenceTime)
            .SetTimestamp(timestamp)
            .SetTraceId(traceId)
            .Instance;

        result.Should().NotBeNull();
        result.AwayTeam.Should().NotBeNull().And.Be(awayTeam);
        result.HomeTeam.Should().NotBeNull().And.Be(homeTeam);
        result.Id.Should().NotBeNull().And.Be(id);
        result.CommenceTime.Should().Be(commenceTime);
        result.Timestamp.Should().Be(timestamp);
        result.TraceId.Should().Be(traceId);
    }

    [Test]
    public void SetAwayTeam_WithValidAwayTeam_ReturnsValidInstance()
    {
        const string awayTeam = nameof(awayTeam);

        var result = new UpcomingEventBuilder().SetAwayTeam(awayTeam).Instance;

        result.Should().NotBeNull();
        result.AwayTeam.Should().Be(awayTeam);
    }

    [Test]
    public void SetHomeTeam_WithValidHomeTeam_ReturnsValidInstance()
    {
        const string homeTeam = nameof(homeTeam);

        var result = new UpcomingEventBuilder().SetHomeTeam(homeTeam).Instance;

        result.Should().NotBeNull();
        result.HomeTeam.Should().NotBeNull().And.Be(homeTeam);
    }

    [Test]
    public void SetId_WithValidId_ReturnsValidInstance()
    {
        const string id = nameof(id);

        var result = new UpcomingEventBuilder().SetId(id).Instance;

        result.Should().NotBeNull();
        result.Id.Should().NotBeNull().And.Be(id);
    }

    [Test]
    public void SetCommenceTime_WithValidCommenceTime_ReturnsValidInstance()
    {
        var commenceTime = DateTime.Now;

        var result = new UpcomingEventBuilder().SetCommenceTime(commenceTime).Instance;

        result.Should().NotBeNull();
        result.CommenceTime.Should().Be(commenceTime);
    }

    [Test]
    public void SetTimestamp_WithValidTimestamp_ReturnsValidInstance()
    {
        var timestamp = DateTime.Now;

        var result = new UpcomingEventBuilder().SetTimestamp(timestamp).Instance;

        result.Should().NotBeNull();
        result.Timestamp.Should().Be(timestamp);
    }

    [Test]
    public void SetTraceId_WithValidTraceId_ReturnsValidInstance()
    {
        var traceId = Guid.NewGuid();

        var result = new UpcomingEventBuilder().SetTraceId(traceId).Instance;

        result.Should().NotBeNull();
        result.TraceId.Should().Be(traceId);
    }

    [TestCase("")]
    [TestCase(null)]
    public void SetAwayTeam_WithNullOrEmptyAwayTeam_ThrowsException(string? awayTeam)
    {
        var action = () =>
        {
            _ = new UpcomingEventBuilder().SetAwayTeam(awayTeam).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(awayTeam));
    }

    [TestCase("")]
    [TestCase(null)]
    public void SetHomeTeam_WithNullOrEmptyHomeTeam_ThrowsException(string? homeTeam)
    {
        var action = () =>
        {
            _ = new UpcomingEventBuilder().SetHomeTeam(homeTeam).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(homeTeam));
    }

    [TestCase("")]
    [TestCase(null)]
    public void SetId_WithNullOrEmptyId_ThrowsException(string? id)
    {
        var action = () =>
        {
            _ = new UpcomingEventBuilder().SetId(id).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(id));
    }

    [Test]
    public void SetCommenceTime_WithNullCommenceTime_ThrowsException()
    {
        var action = () =>
        {
            _ = new UpcomingEventBuilder().SetCommenceTime(null).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName("commenceTime");
    }

    [Test]
    public void SetTimestamp_WithNullTimestamp_ThrowsException()
    {
        var action = () =>
        {
            _ = new UpcomingEventBuilder().SetTimestamp(null).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName("timestamp");
    }

    [Test]
    public void SetTraceId_WithNullTraceId_ThrowsException()
    {
        var action = () =>
        {
            _ = new UpcomingEventBuilder().SetTraceId(null).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName("traceId");
    }
}
