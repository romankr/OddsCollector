using FluentAssertions;
using OddsCollector.Common.Models;
using OddsCollector.Functions.Predictions.Strategies;

namespace OddsCollector.Functions.Predictions.Tests.Strategies;

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
        prediction.Bookmaker.Should().NotBeNull().And.Be("betclic");
        prediction.CommenceTime.Should().Be(UpcomingEventBuilderExtensions.DefaultCommenceTime);
        prediction.HomeTeam.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultHomeTeam);
        prediction.Id.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultId);
        prediction.Strategy.Should().NotBeNull().And.Be(nameof(AdjustedConsensusStrategy));
        prediction.Timestamp.Should().Be(timestamp);
        prediction.TraceId.Should().Be(UpcomingEventBuilderExtensions.DefaultTraceId);
        prediction.Winner.Should().NotBeNull().And.Be("Manchester City");
    }

    [Test]
    public void GetPrediction_WithUpcomingEventWithWinningAwayTeam_ReturnsPrediction()
    {
        var upcomingEvent = new UpcomingEventBuilder().SetDefaults().SetOdds(new List<Odd>
        {
            new() { Away = 1.8, Bookmaker = "betclic", Draw = 3.82, Home = 4.08 },
            new() { Away = 1.7, Bookmaker = "sport888", Draw = 4.33, Home = 4.33 },
            new() { Away = 1.67, Bookmaker = "mybookieag", Draw = 4.5, Home = 4.5 }
        }).Instance;

        var timestamp = DateTime.Now;

        var strategy = new AdjustedConsensusStrategy();

        var prediction = strategy.GetPrediction(upcomingEvent, timestamp);

        prediction.Should().NotBeNull();
        prediction.AwayTeam.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultAwayTeam);
        prediction.Bookmaker.Should().NotBeNull().And.Be("betclic");
        prediction.CommenceTime.Should().Be(UpcomingEventBuilderExtensions.DefaultCommenceTime);
        prediction.HomeTeam.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultHomeTeam);
        prediction.Id.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultId);
        prediction.Strategy.Should().NotBeNull().And.Be(nameof(AdjustedConsensusStrategy));
        prediction.Timestamp.Should().Be(timestamp);
        prediction.TraceId.Should().Be(UpcomingEventBuilderExtensions.DefaultTraceId);
        prediction.Winner.Should().NotBeNull().And.Be("Liverpool");
    }

    [Test]
    public void GetPrediction_WithUpcomingEventWithDraw_ReturnsPrediction()
    {
        var upcomingEvent = new UpcomingEventBuilder().SetDefaults().SetOdds(new List<Odd>
        {
            new() { Away = 3.82, Bookmaker = "betclic", Draw = 1.8, Home = 4.08 },
            new() { Away = 4.33, Bookmaker = "sport888", Draw = 1.7, Home = 4.33 },
            new() { Away = 4.5, Bookmaker = "mybookieag", Draw = 1.67, Home = 4.5 }
        }).Instance;

        var timestamp = DateTime.Now;

        var strategy = new AdjustedConsensusStrategy();

        var prediction = strategy.GetPrediction(upcomingEvent, timestamp);

        prediction.Should().NotBeNull();
        prediction.AwayTeam.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultAwayTeam);
        prediction.Bookmaker.Should().NotBeNull().And.Be("betclic");
        prediction.CommenceTime.Should().Be(UpcomingEventBuilderExtensions.DefaultCommenceTime);
        prediction.HomeTeam.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultHomeTeam);
        prediction.Id.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultId);
        prediction.Strategy.Should().NotBeNull().And.Be(nameof(AdjustedConsensusStrategy));
        prediction.Timestamp.Should().Be(timestamp);
        prediction.TraceId.Should().Be(UpcomingEventBuilderExtensions.DefaultTraceId);
        prediction.Winner.Should().NotBeNull().And.Be("Draw");
    }

    [Test]
    public void GetPrediction_WithValidUpcomingEvent_ReturnsPrediction()
    {
        var upcomingEvent = new UpcomingEventBuilder().SetDefaults().Instance;

        var timestamp = DateTime.Now;

        var strategy = new AdjustedConsensusStrategy();

        var prediction = strategy.GetPrediction(upcomingEvent, timestamp);

        prediction.Should().NotBeNull();
        prediction.AwayTeam.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultAwayTeam);
        prediction.Bookmaker.Should().NotBeNull().And.Be("betclic");
        prediction.CommenceTime.Should().Be(UpcomingEventBuilderExtensions.DefaultCommenceTime);
        prediction.HomeTeam.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultHomeTeam);
        prediction.Id.Should().NotBeNull().And.Be(UpcomingEventBuilderExtensions.DefaultId);
        prediction.Strategy.Should().NotBeNull().And.Be(nameof(AdjustedConsensusStrategy));
        prediction.Timestamp.Should().Be(timestamp);
        prediction.TraceId.Should().Be(UpcomingEventBuilderExtensions.DefaultTraceId);
        prediction.Winner.Should().NotBeNull().And.Be("Manchester City");
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
                    .SetOdds(new List<Odd> { new OddBuilder().SetDefaults().SetAway(null).Instance }).Instance,
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
                    .SetOdds(new List<Odd> { new OddBuilder().SetDefaults().SetBookmaker(bookmaker).Instance })
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
                    .SetOdds(new List<Odd> { new OddBuilder().SetDefaults().SetDraw(null).Instance }).Instance,
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
                    .SetOdds(new List<Odd> { new OddBuilder().SetDefaults().SetHome(null).Instance }).Instance,
                DateTime.Now);
        };

        action.Should().Throw<ArgumentException>().WithParameterName("home");
    }
}
