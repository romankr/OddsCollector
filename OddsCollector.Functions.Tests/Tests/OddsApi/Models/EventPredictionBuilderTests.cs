using OddsCollector.Functions.Models;
using OddsCollector.Functions.Tests.Data;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Models;

[Parallelizable(ParallelScope.All)]
internal class EventPredictionBuilderTests
{
    [Test]
    public void Instance_WithoutParameters_ReturnsValidInstance()
    {
        var result = new EventPredictionBuilder().Instance;

        result.Should().NotBeNull();
    }

    [Test]
    public void Instance_WithFullParameterList_ReturnsValidInstance()
    {
        var result = new EventPredictionBuilder()
            .SetBookmaker(SampleEvent.Bookmaker1)
            .SetWinner(SampleEvent.Winner)
            .SetAwayTeam(SampleEvent.AwayTeam)
            .SetHomeTeam(SampleEvent.HomeTeam)
            .SetId(SampleEvent.Id)
            .SetStrategy(SampleEvent.Strategy)
            .SetCommenceTime(SampleEvent.CommenceTime)
            .SetTimestamp(SampleEvent.Timestamp)
            .SetTraceId(SampleEvent.TraceId)
            .Instance;

        result.Should().NotBeNull();
        result.Bookmaker.Should().NotBeNull().And.Be(SampleEvent.Bookmaker1);
        result.Winner.Should().NotBeNull().And.Be(SampleEvent.Winner);
        result.AwayTeam.Should().NotBeNull().And.Be(SampleEvent.AwayTeam);
        result.HomeTeam.Should().NotBeNull().And.Be(SampleEvent.HomeTeam);
        result.Id.Should().NotBeNull().And.Be(SampleEvent.Id);
        result.Strategy.Should().NotBeNull().And.Be(SampleEvent.Strategy);
        result.CommenceTime.Should().Be(SampleEvent.CommenceTime);
        result.Timestamp.Should().Be(SampleEvent.Timestamp);
        result.TraceId.Should().Be(SampleEvent.TraceId);
    }

    [Test]
    public void SetBookmaker_WithValidBookmaker_ReturnsValidInstance()
    {
        var result = new EventPredictionBuilder().SetBookmaker(SampleEvent.Bookmaker1).Instance;

        result.Should().NotBeNull();
        result.Bookmaker.Should().NotBeNull().And.Be(SampleEvent.Bookmaker1);
    }

    [Test]
    public void SetWinner_WithValidWinner_ReturnsValidInstance()
    {
        var result = new EventPredictionBuilder().SetWinner(SampleEvent.Winner).Instance;

        result.Should().NotBeNull();
        result.Winner.Should().NotBeNull().And.Be(SampleEvent.Winner);
    }

    [Test]
    public void SetAwayTeam_WithValidAwayTeam_ReturnsValidInstance()
    {
        var result = new EventPredictionBuilder().SetAwayTeam(SampleEvent.AwayTeam).Instance;

        result.Should().NotBeNull();
        result.AwayTeam.Should().NotBeNull().And.Be(SampleEvent.AwayTeam);
    }

    [Test]
    public void SetHomeTeam_WithValidHomeTeam_ReturnsValidInstance()
    {
        var result = new EventPredictionBuilder().SetHomeTeam(SampleEvent.HomeTeam).Instance;

        result.Should().NotBeNull();
        result.HomeTeam.Should().NotBeNull().And.Be(SampleEvent.HomeTeam);
    }

    [Test]
    public void SetId_WithValidId_ReturnsValidInstance()
    {
        var result = new EventPredictionBuilder().SetId(SampleEvent.Id).Instance;

        result.Should().NotBeNull();
        result.Id.Should().NotBeNull().And.Be(SampleEvent.Id);
    }

    [Test]
    public void SetStrategy_WithValidStrategy_ReturnsValidInstance()
    {
        var result = new EventPredictionBuilder().SetStrategy(SampleEvent.Strategy).Instance;

        result.Should().NotBeNull();
        result.Strategy.Should().NotBeNull().And.Be(SampleEvent.Strategy);
    }

    [Test]
    public void SetCommenceTime_WithValidCommenceTime_ReturnsValidInstance()
    {
        var result = new EventPredictionBuilder().SetCommenceTime(SampleEvent.CommenceTime).Instance;

        result.Should().NotBeNull();
        result.CommenceTime.Should().Be(SampleEvent.CommenceTime);
    }

    [Test]
    public void SetTimestamp_WithValidTimestamp_ReturnsValidInstance()
    {
        var result = new EventPredictionBuilder().SetTimestamp(SampleEvent.Timestamp).Instance;

        result.Should().NotBeNull();
        result.Timestamp.Should().Be(SampleEvent.Timestamp);
    }

    [Test]
    public void SetTraceId_WithValidTraceId_ReturnsValidInstance()
    {
        var result = new EventPredictionBuilder().SetTraceId(SampleEvent.TraceId).Instance;

        result.Should().NotBeNull();
        result.TraceId.Should().Be(SampleEvent.TraceId);
    }

    [TestCase("")]
    [TestCase(null)]
    public void SetBookmaker_WithNullOrEmptyBookmaker_ThrowsException(string? bookmaker)
    {
        var action = () =>
        {
            _ = new EventPredictionBuilder().SetBookmaker(bookmaker).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(bookmaker));
    }

    [TestCase("")]
    [TestCase(null)]
    public void SetWinner_WithNullOrEmptyWinner_ThrowsException(string? winner)
    {
        var action = () =>
        {
            _ = new EventPredictionBuilder().SetWinner(winner).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(winner));
    }

    [TestCase("")]
    [TestCase(null)]
    public void SetAwayTeam_WithNullOrEmptyAwayTeam_ThrowsException(string? awayTeam)
    {
        var action = () =>
        {
            _ = new EventPredictionBuilder().SetAwayTeam(awayTeam).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(awayTeam));
    }

    [TestCase("")]
    [TestCase(null)]
    public void SetHomeTeam_WithNullOrEmptyHomeTeam_ThrowsException(string? homeTeam)
    {
        var action = () =>
        {
            _ = new EventPredictionBuilder().SetHomeTeam(homeTeam).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(homeTeam));
    }

    [TestCase("")]
    [TestCase(null)]
    public void SetId_WithNullOrEmptyId_ThrowsException(string? id)
    {
        var action = () =>
        {
            _ = new EventPredictionBuilder().SetId(id).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(id));
    }

    [TestCase("")]
    [TestCase(null)]
    public void SetStrategy_WithNullOrEmptyStrategy_ThrowsException(string? strategy)
    {
        var action = () =>
        {
            _ = new EventPredictionBuilder().SetStrategy(strategy).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(strategy));
    }

    [Test]
    public void SetCommenceTime_WithNullCommenceTime_ThrowsException()
    {
        var action = () =>
        {
            _ = new EventPredictionBuilder().SetCommenceTime(null).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName("commenceTime");
    }

    [Test]
    public void SetTimestamp_WithNullTimestamp_ThrowsException()
    {
        var action = () =>
        {
            _ = new EventPredictionBuilder().SetTimestamp(null).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName("timestamp");
    }

    [Test]
    public void SetTraceId_WithNullTraceId_ThrowsException()
    {
        var action = () =>
        {
            _ = new EventPredictionBuilder().SetTraceId(null).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName("traceId");
    }
}
