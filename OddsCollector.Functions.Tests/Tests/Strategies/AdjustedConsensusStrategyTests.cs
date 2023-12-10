using OddsCollector.Functions.Models;
using OddsCollector.Functions.Strategies;
using OddsCollector.Functions.Tests.Data;

namespace OddsCollector.Functions.Tests.Tests.Strategies;

[Parallelizable(ParallelScope.All)]
internal class AdjustedConsensusStrategyTests
{
    [Test]
    public void GetPrediction_WithUpcomingEventWithWinningHomeTeam_ReturnsPrediction()
    {
        var upcomingEvent = new UpcomingEventBuilder().SetSampleData().Instance;

        var strategy = new AdjustedConsensusStrategy();

        var prediction = strategy.GetPrediction(upcomingEvent, SampleEvent.Timestamp);

        prediction.Should().NotBeNull();
        prediction.AwayTeam.Should().NotBeNull().And.Be(SampleEvent.AwayTeam);
        prediction.Bookmaker.Should().NotBeNull().And.Be(SampleEvent.Bookmaker1);
        prediction.CommenceTime.Should().Be(SampleEvent.CommenceTime);
        prediction.HomeTeam.Should().NotBeNull().And.Be(SampleEvent.HomeTeam);
        prediction.Id.Should().NotBeNull().And.Be(SampleEvent.Id);
        prediction.Strategy.Should().NotBeNull().And.Be(SampleEvent.Strategy);
        prediction.Timestamp.Should().Be(SampleEvent.Timestamp);
        prediction.TraceId.Should().Be(SampleEvent.TraceId);
        prediction.Winner.Should().NotBeNull().And.Be(SampleEvent.HomeTeam);
    }

    [Test]
    public void GetPrediction_WithUpcomingEventWithWinningAwayTeam_ReturnsPrediction()
    {
        var upcomingEvent = new UpcomingEventBuilder().SetSampleData().SetOdds(new List<Odd>
        {
            new OddBuilder().SetSampleData1().SetAway(SampleEvent.HomeOdd1).SetHome(SampleEvent.AwayOdd1).Instance,
            new OddBuilder().SetSampleData2().SetAway(SampleEvent.HomeOdd2).SetHome(SampleEvent.AwayOdd2).Instance,
            new OddBuilder().SetSampleData3().SetAway(SampleEvent.HomeOdd3).SetHome(SampleEvent.AwayOdd3).Instance
        }).Instance;

        var strategy = new AdjustedConsensusStrategy();

        var prediction = strategy.GetPrediction(upcomingEvent, SampleEvent.Timestamp);

        prediction.Should().NotBeNull();
        prediction.AwayTeam.Should().NotBeNull().And.Be(SampleEvent.AwayTeam);
        prediction.Bookmaker.Should().NotBeNull().And.Be(SampleEvent.Bookmaker1);
        prediction.CommenceTime.Should().Be(SampleEvent.CommenceTime);
        prediction.HomeTeam.Should().NotBeNull().And.Be(SampleEvent.HomeTeam);
        prediction.Id.Should().NotBeNull().And.Be(SampleEvent.Id);
        prediction.Strategy.Should().NotBeNull().And.Be(SampleEvent.Strategy);
        prediction.Timestamp.Should().Be(SampleEvent.Timestamp);
        prediction.TraceId.Should().Be(SampleEvent.TraceId);
        prediction.Winner.Should().NotBeNull().And.Be(SampleEvent.AwayTeam);
    }

    [Test]
    public void GetPrediction_WithUpcomingEventWithDraw_ReturnsPrediction()
    {
        var upcomingEvent = new UpcomingEventBuilder().SetSampleData().SetOdds(new List<Odd>
        {
            new OddBuilder().SetSampleData1().SetDraw(SampleEvent.HomeOdd1).SetHome(SampleEvent.DrawOdd1).Instance,
            new OddBuilder().SetSampleData2().SetDraw(SampleEvent.HomeOdd2).SetHome(SampleEvent.DrawOdd2).Instance,
            new OddBuilder().SetSampleData3().SetDraw(SampleEvent.HomeOdd3).SetHome(SampleEvent.DrawOdd3).Instance
        }).Instance;

        var strategy = new AdjustedConsensusStrategy();

        var prediction = strategy.GetPrediction(upcomingEvent, SampleEvent.Timestamp);

        prediction.Should().NotBeNull();
        prediction.AwayTeam.Should().NotBeNull().And.Be(SampleEvent.AwayTeam);
        prediction.Bookmaker.Should().NotBeNull().And.Be(SampleEvent.Bookmaker1);
        prediction.CommenceTime.Should().Be(SampleEvent.CommenceTime);
        prediction.HomeTeam.Should().NotBeNull().And.Be(SampleEvent.HomeTeam);
        prediction.Id.Should().NotBeNull().And.Be(SampleEvent.Id);
        prediction.Strategy.Should().NotBeNull().And.Be(nameof(AdjustedConsensusStrategy));
        prediction.Timestamp.Should().Be(SampleEvent.Timestamp);
        prediction.TraceId.Should().Be(SampleEvent.TraceId);
        prediction.Winner.Should().NotBeNull().And.Be(Constants.Draw);
    }

    [Test]
    public void GetPrediction_WithDifferentWinningBookmaker_ReturnsPrediction()
    {
        var upcomingEvent = new UpcomingEventBuilder().SetSampleData().SetOdds(new List<Odd>
        {
            new OddBuilder().SetSampleData1().SetBookmaker(SampleEvent.Bookmaker2).Instance,
            new OddBuilder().SetSampleData2().SetBookmaker(SampleEvent.Bookmaker1).Instance,
            new OddBuilder().SetSampleData3().SetBookmaker(SampleEvent.Bookmaker3).Instance
        }).Instance;

        var strategy = new AdjustedConsensusStrategy();

        var prediction = strategy.GetPrediction(upcomingEvent, SampleEvent.Timestamp);

        prediction.Should().NotBeNull();
        prediction.AwayTeam.Should().NotBeNull().And.Be(SampleEvent.AwayTeam);
        prediction.Bookmaker.Should().NotBeNull().And.Be(SampleEvent.Bookmaker2);
        prediction.CommenceTime.Should().Be(SampleEvent.CommenceTime);
        prediction.HomeTeam.Should().NotBeNull().And.Be(SampleEvent.HomeTeam);
        prediction.Id.Should().NotBeNull().And.Be(SampleEvent.Id);
        prediction.Strategy.Should().NotBeNull().And.Be(SampleEvent.Strategy);
        prediction.Timestamp.Should().Be(SampleEvent.Timestamp);
        prediction.TraceId.Should().Be(SampleEvent.TraceId);
        prediction.Winner.Should().NotBeNull().And.Be(SampleEvent.HomeTeam);
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
            _ = strategy.GetPrediction(new UpcomingEventBuilder().SetSampleData().Instance, null);
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
            _ = strategy.GetPrediction(new UpcomingEventBuilder().SetSampleData().SetId(id).Instance, DateTime.Now);
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(id));
    }

    [Test]
    public void GetPrediction_WithNullEventTimestamp_ThrowsException()
    {
        var strategy = new AdjustedConsensusStrategy();

        var action = () =>
        {
            _ = strategy.GetPrediction(new UpcomingEventBuilder().SetSampleData().SetTimestamp(null).Instance,
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
            _ = strategy.GetPrediction(new UpcomingEventBuilder().SetSampleData().SetTraceId(null).Instance,
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
            _ = strategy.GetPrediction(new UpcomingEventBuilder().SetSampleData().SetCommenceTime(null).Instance,
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
            _ = strategy.GetPrediction(new UpcomingEventBuilder().SetSampleData().SetAwayTeam(null).Instance,
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
            _ = strategy.GetPrediction(new UpcomingEventBuilder().SetSampleData().SetHomeTeam(null).Instance,
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
            _ = strategy.GetPrediction(new UpcomingEventBuilder().SetSampleData().SetOdds(null).Instance, DateTime.Now);
        };

        action.Should().Throw<ArgumentException>().WithParameterName("odds");
    }

    [Test]
    public void GetPrediction_WithEmptyOdds_ThrowsException()
    {
        var strategy = new AdjustedConsensusStrategy();

        var action = () =>
        {
            _ = strategy.GetPrediction(new UpcomingEventBuilder().SetSampleData().SetOdds(new List<Odd>()).Instance,
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
                new UpcomingEventBuilder().SetSampleData()
                    .SetOdds(new List<Odd> { new OddBuilder().SetSampleData1().SetAway(null).Instance }).Instance,
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
                new UpcomingEventBuilder().SetSampleData()
                    .SetOdds(new List<Odd> { new OddBuilder().SetSampleData1().SetBookmaker(bookmaker).Instance })
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
                new UpcomingEventBuilder().SetSampleData()
                    .SetOdds(new List<Odd> { new OddBuilder().SetSampleData1().SetDraw(null).Instance }).Instance,
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
                new UpcomingEventBuilder().SetSampleData()
                    .SetOdds(new List<Odd> { new OddBuilder().SetSampleData1().SetHome(null).Instance }).Instance,
                DateTime.Now);
        };

        action.Should().Throw<ArgumentException>().WithParameterName("home");
    }
}
