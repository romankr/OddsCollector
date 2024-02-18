using OddsCollector.Functions.Models;
using OddsCollector.Functions.Strategies;
using OddsCollector.Functions.Tests.Data;

namespace OddsCollector.Functions.Tests.Tests.Strategies;

[Parallelizable(ParallelScope.All)]
internal class AdjustedConsensusStrategyTests
{
    private static readonly IEnumerable<(UpcomingEvent UpcomingEvent, EventPrediction EventPrediction)>
        TestCases = new List<(UpcomingEvent UpcomingEvent, EventPrediction EventPrediction)>
        {
            // Default winner - sample home team
            (
                new UpcomingEventBuilder().SetSampleData().Instance,
                new EventPredictionBuilder().SetSampleData().Instance
            ),
            // The winner is away team
            (
                new UpcomingEventBuilder().SetSampleData().SetOdds(new List<Odd>
                {
                    new OddBuilder().SetSampleData1().SetAway(SampleEvent.HomeOdd1).SetHome(SampleEvent.AwayOdd1).Instance,
                    new OddBuilder().SetSampleData2().SetAway(SampleEvent.HomeOdd2).SetHome(SampleEvent.AwayOdd2).Instance,
                    new OddBuilder().SetSampleData3().SetAway(SampleEvent.HomeOdd3).SetHome(SampleEvent.AwayOdd3).Instance
                }).Instance,
                new EventPredictionBuilder().SetSampleData().SetWinner(SampleEvent.AwayTeam).Instance
            ),
            // The outcome is draw
            (
                new UpcomingEventBuilder().SetSampleData().SetOdds(new List<Odd>
                {
                    new OddBuilder().SetSampleData1().SetDraw(SampleEvent.HomeOdd1).SetHome(SampleEvent.DrawOdd1).Instance,
                    new OddBuilder().SetSampleData2().SetDraw(SampleEvent.HomeOdd2).SetHome(SampleEvent.DrawOdd2).Instance,
                    new OddBuilder().SetSampleData3().SetDraw(SampleEvent.HomeOdd3).SetHome(SampleEvent.DrawOdd3).Instance
                }).Instance,
                new EventPredictionBuilder().SetSampleData().SetWinner(Constants.Draw).Instance
            ),
            // Another bookmaker has the best odds
            (
                new UpcomingEventBuilder().SetSampleData().SetOdds(new List<Odd>
                {
                    new OddBuilder().SetSampleData1().SetBookmaker(SampleEvent.Bookmaker2).Instance,
                    new OddBuilder().SetSampleData2().SetBookmaker(SampleEvent.Bookmaker1).Instance,
                    new OddBuilder().SetSampleData3().SetBookmaker(SampleEvent.Bookmaker3).Instance
                }).Instance,
                new EventPredictionBuilder().SetSampleData().SetBookmaker(SampleEvent.Bookmaker2).Instance
            )
        };

    [TestCaseSource(nameof(TestCases))]
    public void GetPrediction_WithUpcomingEvent_ReturnsPrediction(
        (UpcomingEvent UpcomingEvent, EventPrediction EventPrediction) testCase)
    {
        // Arrange
        var strategy = new AdjustedConsensusStrategy();

        // Act
        var prediction = strategy.GetPrediction(testCase.UpcomingEvent, SampleEvent.Timestamp);

        // Assert
        prediction.Should().NotBeNull().And.BeEquivalentTo(testCase.EventPrediction);
    }

    [Test]
    public void GetPrediction_WithNullUpcomingEvent_ThrowsException()
    {
        // Arrange
        var strategy = new AdjustedConsensusStrategy();

        // Act
        var action = () => strategy.GetPrediction(null, DateTime.Now);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("upcomingEvent");
    }

    [Test]
    public void GetPrediction_WithNullTimestamp_ThrowsException()
    {
        // Arrange
        var strategy = new AdjustedConsensusStrategy();

        // Act
        var action = () => strategy.GetPrediction(new UpcomingEvent(), null);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("timestamp");
    }

    [Test]
    public void GetPrediction_WithEmptyOdds_ThrowsException()
    {
        // Arrange
        var strategy = new AdjustedConsensusStrategy();

        // Act
        var action = () =>
            strategy.GetPrediction(new UpcomingEventBuilder().SetSampleData().SetOdds(new List<Odd>()).Instance, DateTime.Now);

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("odds");
    }
}
