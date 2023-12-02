using OddsCollector.Functions.Models;

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
        const string bookmaker = nameof(bookmaker);
        const string winner = nameof(winner);
        const string awayTeam = nameof(awayTeam);
        const string homeTeam = nameof(homeTeam);
        const string id = nameof(id);
        const string strategy = nameof(strategy);
        var commenceTime = DateTime.UtcNow;
        var timestamp = DateTime.UtcNow;
        var traceId = Guid.NewGuid();

        var result = new EventPredictionBuilder()
            .SetBookmaker(bookmaker)
            .SetWinner(winner)
            .SetAwayTeam(awayTeam)
            .SetHomeTeam(homeTeam)
            .SetId(id)
            .SetStrategy(strategy)
            .SetCommenceTime(commenceTime)
            .SetTimestamp(timestamp)
            .SetTraceId(traceId)
            .Instance;

        result.Should().NotBeNull();
        result.Bookmaker.Should().NotBeNull().And.Be(bookmaker);
        result.Winner.Should().NotBeNull().And.Be(winner);
        result.AwayTeam.Should().NotBeNull().And.Be(awayTeam);
        result.HomeTeam.Should().NotBeNull().And.Be(homeTeam);
        result.Id.Should().NotBeNull().And.Be(id);
        result.Strategy.Should().NotBeNull().And.Be(strategy);
        result.CommenceTime.Should().Be(commenceTime);
        result.Timestamp.Should().Be(timestamp);
        result.TraceId.Should().Be(traceId);
    }

    [Test]
    public void SetBookmaker_WithValidBookmaker_ReturnsValidInstance()
    {
        const string bookmaker = nameof(bookmaker);

        var result = new EventPredictionBuilder().SetBookmaker(bookmaker).Instance;

        result.Should().NotBeNull();
        result.Bookmaker.Should().NotBeNull().And.Be(bookmaker);
    }

    [Test]
    public void SetWinner_WithValidWinner_ReturnsValidInstance()
    {
        const string winner = nameof(winner);

        var result = new EventPredictionBuilder().SetWinner(winner).Instance;

        result.Should().NotBeNull();
        result.Winner.Should().NotBeNull().And.Be(winner);
    }

    [Test]
    public void SetAwayTeam_WithValidAwayTeam_ReturnsValidInstance()
    {
        const string awayTeam = nameof(awayTeam);

        var result = new EventPredictionBuilder().SetAwayTeam(awayTeam).Instance;

        result.Should().NotBeNull();
        result.AwayTeam.Should().NotBeNull().And.Be(awayTeam);
    }

    [Test]
    public void SetHomeTeam_WithValidHomeTeam_ReturnsValidInstance()
    {
        const string homeTeam = nameof(homeTeam);

        var result = new EventPredictionBuilder().SetHomeTeam(homeTeam).Instance;

        result.Should().NotBeNull();
        result.HomeTeam.Should().NotBeNull().And.Be(homeTeam);
    }

    [Test]
    public void SetId_WithValidId_ReturnsValidInstance()
    {
        const string id = nameof(id);

        var result = new EventPredictionBuilder().SetId(id).Instance;

        result.Should().NotBeNull();
        result.Id.Should().NotBeNull().And.Be(id);
    }

    [Test]
    public void SetStrategy_WithValidStrategy_ReturnsValidInstance()
    {
        const string strategy = nameof(strategy);

        var result = new EventPredictionBuilder().SetStrategy(strategy).Instance;

        result.Should().NotBeNull();
        result.Strategy.Should().NotBeNull().And.Be(strategy);
    }

    [Test]
    public void SetCommenceTime_WithValidCommenceTime_ReturnsValidInstance()
    {
        var commenceTime = DateTime.Now;

        var result = new EventPredictionBuilder().SetCommenceTime(commenceTime).Instance;

        result.Should().NotBeNull();
        result.CommenceTime.Should().Be(commenceTime);
    }

    [Test]
    public void SetTimestamp_WithValidTimestamp_ReturnsValidInstance()
    {
        var timestamp = DateTime.Now;

        var result = new EventPredictionBuilder().SetTimestamp(timestamp).Instance;

        result.Should().NotBeNull();
        result.Timestamp.Should().Be(timestamp);
    }

    [Test]
    public void SetTraceId_WithValidTraceId_ReturnsValidInstance()
    {
        var traceId = Guid.NewGuid();

        var result = new EventPredictionBuilder().SetTraceId(traceId).Instance;

        result.Should().NotBeNull();
        result.TraceId.Should().Be(traceId);
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
