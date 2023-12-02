using OddsCollector.Functions.Models;
using OddsCollector.Functions.Strategies;
using OddsCollector.Functions.Tests.Common.Models;

namespace OddsCollector.Functions.Tests.Tests.Strategies;

[Parallelizable(ParallelScope.All)]
internal class AdjustedConsensusStrategyTests
{
    [Test]
    public void GetPrediction_WithUpcomingEventWithWinningHomeTeam_ReturnsPrediction()
    {
        var upcomingEvent = new UpcomingEventBuilder().SetDefaults().Instance;

        var timestamp = DateTime.Now;

        var strategy = new AdjustedConsensusStrategy();

        var prediction = strategy.GetPrediction(upcomingEvent, timestamp);

        prediction.Should().NotBeNull();
        prediction.AwayTeam.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultAwayTeam);
        prediction.Bookmaker.Should().NotBeNull().And.Be(OddsBuilderExtensions.DefaultBookmaker1);
        prediction.CommenceTime.Should().Be(UpcomingEventBuilderExtensions.DefaultCommenceTime);
        prediction.HomeTeam.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultHomeTeam);
        prediction.Id.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultId);
        prediction.Strategy.Should().NotBeNull().And.Be(nameof(AdjustedConsensusStrategy));
        prediction.Timestamp.Should().Be(timestamp);
        prediction.TraceId.Should().Be(UpcomingEventBuilderExtensions.DefaultTraceId);
        prediction.Winner.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultHomeTeam);
    }

    [Test]
    public void GetPrediction_WithUpcomingEventWithWinningAwayTeam_ReturnsPrediction()
    {
        var upcomingEvent = new UpcomingEventBuilder().SetDefaults().SetOdds(new List<Odd>
        {
            new() { Away = 1.8, Bookmaker = OddsBuilderExtensions.DefaultBookmaker1, Draw = 3.82, Home = 4.08 },
            new() { Away = 1.7, Bookmaker = OddsBuilderExtensions.DefaultBookmaker2, Draw = 4.33, Home = 4.33 },
            new() { Away = 1.67, Bookmaker = OddsBuilderExtensions.DefaultBookmaker3, Draw = 4.5, Home = 4.5 }
        }).Instance;

        var timestamp = DateTime.Now;

        var strategy = new AdjustedConsensusStrategy();

        var prediction = strategy.GetPrediction(upcomingEvent, timestamp);

        prediction.Should().NotBeNull();
        prediction.AwayTeam.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultAwayTeam);
        prediction.Bookmaker.Should().NotBeNull().And.Be(OddsBuilderExtensions.DefaultBookmaker1);
        prediction.CommenceTime.Should().Be(UpcomingEventBuilderExtensions.DefaultCommenceTime);
        prediction.HomeTeam.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultHomeTeam);
        prediction.Id.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultId);
        prediction.Strategy.Should().NotBeNull().And.Be(nameof(AdjustedConsensusStrategy));
        prediction.Timestamp.Should().Be(timestamp);
        prediction.TraceId.Should().Be(UpcomingEventBuilderExtensions.DefaultTraceId);
        prediction.Winner.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultAwayTeam);
    }

    [Test]
    public void GetPrediction_WithUpcomingEventWithDraw_ReturnsPrediction()
    {
        var upcomingEvent = new UpcomingEventBuilder().SetDefaults().SetOdds(new List<Odd>
        {
            new() { Away = 3.82, Bookmaker = OddsBuilderExtensions.DefaultBookmaker1, Draw = 1.8, Home = 4.08 },
            new() { Away = 4.33, Bookmaker = OddsBuilderExtensions.DefaultBookmaker2, Draw = 1.7, Home = 4.33 },
            new() { Away = 4.5, Bookmaker = OddsBuilderExtensions.DefaultBookmaker3, Draw = 1.67, Home = 4.5 }
        }).Instance;

        var timestamp = DateTime.Now;

        var strategy = new AdjustedConsensusStrategy();

        var prediction = strategy.GetPrediction(upcomingEvent, timestamp);

        prediction.Should().NotBeNull();
        prediction.AwayTeam.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultAwayTeam);
        prediction.Bookmaker.Should().NotBeNull().And.Be(OddsBuilderExtensions.DefaultBookmaker1);
        prediction.CommenceTime.Should().Be(UpcomingEventBuilderExtensions.DefaultCommenceTime);
        prediction.HomeTeam.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultHomeTeam);
        prediction.Id.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultId);
        prediction.Strategy.Should().NotBeNull().And.Be(nameof(AdjustedConsensusStrategy));
        prediction.Timestamp.Should().Be(timestamp);
        prediction.TraceId.Should().Be(UpcomingEventBuilderExtensions.DefaultTraceId);
        prediction.Winner.Should().NotBeNull().And.Be(Constants.Draw);
    }

    [Test]
    public void GetPrediction_WithDifferentWinningBookmaker_ReturnsPrediction()
    {
        var upcomingEvent = new UpcomingEventBuilder().SetDefaults().SetOdds(new List<Odd>
        {
            new() { Away = 3.82, Bookmaker = OddsBuilderExtensions.DefaultBookmaker2, Draw = 1.8, Home = 4.08 },
            new() { Away = 4.33, Bookmaker = OddsBuilderExtensions.DefaultBookmaker1, Draw = 1.7, Home = 4.33 },
            new() { Away = 4.5, Bookmaker = OddsBuilderExtensions.DefaultBookmaker3, Draw = 1.67, Home = 4.5 }
        }).Instance;

        var timestamp = DateTime.Now;

        var strategy = new AdjustedConsensusStrategy();

        var prediction = strategy.GetPrediction(upcomingEvent, timestamp);

        prediction.Should().NotBeNull();
        prediction.AwayTeam.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultAwayTeam);
        prediction.Bookmaker.Should().NotBeNull().And.Be("sport888");
        prediction.CommenceTime.Should().Be(UpcomingEventBuilderExtensions.DefaultCommenceTime);
        prediction.HomeTeam.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultHomeTeam);
        prediction.Id.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultId);
        prediction.Strategy.Should().NotBeNull().And.Be(nameof(AdjustedConsensusStrategy));
        prediction.Timestamp.Should().Be(timestamp);
        prediction.TraceId.Should().Be(UpcomingEventBuilderExtensions.DefaultTraceId);
        prediction.Winner.Should().NotBeNull().And.Be(Constants.Draw);
    }

    [Test]
    public void GetPrediction_WithNullUpcomingEvent_ThrowsException()
    {
        var strategy = new AdjustedConsensusStrategy();

        var action = () =>
        {
            _ = strategy.GetPrediction(null, DateTime.Now);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("upcomingEvent");
    }

    [Test]
    public void GetPrediction_WithNullTimestamp_ThrowsException()
    {
        var strategy = new AdjustedConsensusStrategy();

        var action = () =>
        {
            _ = strategy.GetPrediction(new UpcomingEventBuilder().SetDefaults().Instance, null);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("timestamp");
    }

    [TestCase("")]
    [TestCase(null)]
    public void GetPrediction_WithNullOrEmptyId_ThrowsException(string? id)
    {
        var strategy = new AdjustedConsensusStrategy();

        var action = () =>
        {
            _ = strategy.GetPrediction(new UpcomingEventBuilder().SetDefaults().SetId(id).Instance, DateTime.Now);
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(id));
    }

    [Test]
    public void GetPrediction_WithNullEventTimestamp_ThrowsException()
    {
        var strategy = new AdjustedConsensusStrategy();

        var action = () =>
        {
            _ = strategy.GetPrediction(new UpcomingEventBuilder().SetDefaults().SetTimestamp(null).Instance,
                DateTime.Now);
        };

        action.Should().Throw<ArgumentException>().WithParameterName("timestamp");
    }

    [Test]
    public void GetPrediction_WithNullEventTraceId_ThrowsException()
    {
        var strategy = new AdjustedConsensusStrategy();

        var action = () =>
        {
            _ = strategy.GetPrediction(new UpcomingEventBuilder().SetDefaults().SetTraceId(null).Instance,
                DateTime.Now);
        };

        action.Should().Throw<ArgumentException>().WithParameterName("traceId");
    }

    [Test]
    public void GetPrediction_WithNullEventCommenceTime_ThrowsException()
    {
        var strategy = new AdjustedConsensusStrategy();

        var action = () =>
        {
            _ = strategy.GetPrediction(new UpcomingEventBuilder().SetDefaults().SetCommenceTime(null).Instance,
                DateTime.Now);
        };

        action.Should().Throw<ArgumentException>().WithParameterName("commenceTime");
    }

    [TestCase("")]
    [TestCase(null)]
    public void GetPrediction_WithNullEventAwayTeam_ThrowsException(string? awayTeam)
    {
        var strategy = new AdjustedConsensusStrategy();

        var action = () =>
        {
            _ = strategy.GetPrediction(new UpcomingEventBuilder().SetDefaults().SetAwayTeam(null).Instance,
                DateTime.Now);
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(awayTeam));
    }

    [TestCase("")]
    [TestCase(null)]
    public void GetPrediction_WithNullEventHomeTeam_ThrowsException(string? homeTeam)
    {
        var strategy = new AdjustedConsensusStrategy();

        var action = () =>
        {
            _ = strategy.GetPrediction(new UpcomingEventBuilder().SetDefaults().SetHomeTeam(null).Instance,
                DateTime.Now);
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(homeTeam));
    }

    [Test]
    public void GetPrediction_WithNullOdds_ThrowsException()
    {
        var strategy = new AdjustedConsensusStrategy();

        var action = () =>
        {
            _ = strategy.GetPrediction(new UpcomingEventBuilder().SetDefaults().SetOdds(null).Instance, DateTime.Now);
        };

        action.Should().Throw<ArgumentException>().WithParameterName("odds");
    }

    [Test]
    public void GetPrediction_WithEmptyOdds_ThrowsException()
    {
        var strategy = new AdjustedConsensusStrategy();

        var action = () =>
        {
            _ = strategy.GetPrediction(new UpcomingEventBuilder().SetDefaults().SetOdds(new List<Odd>()).Instance,
                DateTime.Now);
        };

        action.Should().Throw<ArgumentException>().WithParameterName("odds");
    }

    [Test]
    public void GetPrediction_WithNullAwayScoreInOdds_ThrowsException()
    {
        var strategy = new AdjustedConsensusStrategy();

        var action = () =>
        {
            _ = strategy.GetPrediction(
                new UpcomingEventBuilder().SetDefaults()
                    .SetOdds(new List<Odd> { new OddBuilder().SetDefaults1().SetAway(null).Instance }).Instance,
                DateTime.Now);
        };

        action.Should().Throw<ArgumentException>().WithParameterName("away");
    }

    [TestCase("")]
    [TestCase(null)]
    public void GetPrediction_WithNullOrEmptyBookmakerInOdds_ThrowsException(string? bookmaker)
    {
        var strategy = new AdjustedConsensusStrategy();

        var action = () =>
        {
            _ = strategy.GetPrediction(
                new UpcomingEventBuilder().SetDefaults()
                    .SetOdds(new List<Odd> { new OddBuilder().SetDefaults1().SetBookmaker(bookmaker).Instance })
                    .Instance, DateTime.Now);
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(bookmaker));
    }

    [Test]
    public void GetPrediction_WithNullDrawScoreInOdds_ThrowsException()
    {
        var strategy = new AdjustedConsensusStrategy();

        var action = () =>
        {
            _ = strategy.GetPrediction(
                new UpcomingEventBuilder().SetDefaults()
                    .SetOdds(new List<Odd> { new OddBuilder().SetDefaults1().SetDraw(null).Instance }).Instance,
                DateTime.Now);
        };

        action.Should().Throw<ArgumentException>().WithParameterName("draw");
    }

    [Test]
    public void GetPrediction_WithNullHomeScoreInOdds_ThrowsException()
    {
        var strategy = new AdjustedConsensusStrategy();

        var action = () =>
        {
            _ = strategy.GetPrediction(
                new UpcomingEventBuilder().SetDefaults()
                    .SetOdds(new List<Odd> { new OddBuilder().SetDefaults1().SetHome(null).Instance }).Instance,
                DateTime.Now);
        };

        action.Should().Throw<ArgumentException>().WithParameterName("home");
    }
}
