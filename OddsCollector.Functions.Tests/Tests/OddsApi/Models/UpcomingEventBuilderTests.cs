using OddsCollector.Functions.Models;
using OddsCollector.Functions.Tests.Data;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Models;

[Parallelizable(ParallelScope.All)]
internal class UpcomingEventBuilderTests
{
    [Test]
    public void Instance_WithoutParameters_ReturnsValidInstance()
    {
        var result = new UpcomingEventBuilder().Instance;

        result.Should().NotBeNull();
    }

    [Test]
    public void Instance_WithFullParameterList_ReturnsValidInstance()
    {
        var result = new UpcomingEventBuilder()
            .SetAwayTeam(SampleEvent.AwayTeam)
            .SetHomeTeam(SampleEvent.HomeTeam)
            .SetId(SampleEvent.Id)
            .SetCommenceTime(SampleEvent.CommenceTime)
            .SetTimestamp(SampleEvent.Timestamp)
            .SetTraceId(SampleEvent.TraceId)
            .SetOdds(SampleEvent.Odds)
            .Instance;

        result.Should().NotBeNull();
        result.AwayTeam.Should().NotBeNull().And.Be(SampleEvent.AwayTeam);
        result.HomeTeam.Should().NotBeNull().And.Be(SampleEvent.HomeTeam);
        result.Id.Should().NotBeNull().And.Be(SampleEvent.Id);
        result.CommenceTime.Should().Be(SampleEvent.CommenceTime);
        result.Timestamp.Should().Be(SampleEvent.Timestamp);
        result.TraceId.Should().Be(SampleEvent.TraceId);
        result.Odds.Should().NotBeNull().And.HaveCount(3);

        var first = result.Odds.ElementAt(0);

        first.Should().NotBeNull();
        first.Bookmaker.Should().NotBeNull().And.Be(SampleEvent.Bookmaker1);
        first.Away.Should().Be(SampleEvent.AwayOdd1);
        first.Home.Should().Be(SampleEvent.HomeOdd1);
        first.Draw.Should().Be(SampleEvent.DrawOdd1);

        var second = result.Odds.ElementAt(1);

        second.Should().NotBeNull();
        second.Bookmaker.Should().NotBeNull().And.Be(SampleEvent.Bookmaker2);
        second.Away.Should().Be(SampleEvent.AwayOdd2);
        second.Home.Should().Be(SampleEvent.HomeOdd2);
        second.Draw.Should().Be(SampleEvent.DrawOdd2);

        var third = result.Odds.ElementAt(2);

        third.Should().NotBeNull();
        third.Bookmaker.Should().NotBeNull().And.Be(SampleEvent.Bookmaker3);
        third.Away.Should().Be(SampleEvent.AwayOdd3);
        third.Home.Should().Be(SampleEvent.HomeOdd3);
        third.Draw.Should().Be(SampleEvent.DrawOdd3);
    }

    [Test]
    public void SetAwayTeam_WithValidAwayTeam_ReturnsValidInstance()
    {
        var result = new UpcomingEventBuilder().SetAwayTeam(SampleEvent.AwayTeam).Instance;

        result.Should().NotBeNull();
        result.AwayTeam.Should().Be(SampleEvent.AwayTeam);
    }

    [Test]
    public void SetHomeTeam_WithValidHomeTeam_ReturnsValidInstance()
    {
        var result = new UpcomingEventBuilder().SetHomeTeam(SampleEvent.HomeTeam).Instance;

        result.Should().NotBeNull();
        result.HomeTeam.Should().NotBeNull().And.Be(SampleEvent.HomeTeam);
    }

    [Test]
    public void SetId_WithValidId_ReturnsValidInstance()
    {
        var result = new UpcomingEventBuilder().SetId(SampleEvent.Id).Instance;

        result.Should().NotBeNull();
        result.Id.Should().NotBeNull().And.Be(SampleEvent.Id);
    }

    [Test]
    public void SetCommenceTime_WithValidCommenceTime_ReturnsValidInstance()
    {
        var result = new UpcomingEventBuilder().SetCommenceTime(SampleEvent.CommenceTime).Instance;

        result.Should().NotBeNull();
        result.CommenceTime.Should().Be(SampleEvent.CommenceTime);
    }

    [Test]
    public void SetTimestamp_WithValidTimestamp_ReturnsValidInstance()
    {
        var result = new UpcomingEventBuilder().SetTimestamp(SampleEvent.Timestamp).Instance;

        result.Should().NotBeNull();
        result.Timestamp.Should().Be(SampleEvent.Timestamp);
    }

    [Test]
    public void SetTraceId_WithValidTraceId_ReturnsValidInstance()
    {
        var result = new UpcomingEventBuilder().SetTraceId(SampleEvent.TraceId).Instance;

        result.Should().NotBeNull();
        result.TraceId.Should().Be(SampleEvent.TraceId);
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

    [Test]
    public void SetOdds_WithNullOdds_ThrowsException()
    {
        var action = () =>
        {
            _ = new UpcomingEventBuilder().SetOdds(null).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName("odds");
    }
}
