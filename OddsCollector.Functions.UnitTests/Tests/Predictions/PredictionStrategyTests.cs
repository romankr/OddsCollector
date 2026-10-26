using OddsCollector.Functions.Models;
using OddsCollector.Functions.Predictions;
using OddsCollector.Functions.UnitTests.Infrastructure.Models;

namespace OddsCollector.Functions.UnitTests.Tests.Predictions;

internal sealed class PredictionStrategyTests
{
    [Test]
    public void GetPrediction_WithUpcomingEvent_ReturnsPredictionForIt()
    {
        // Arrange
        var upcomingEvent = new UpcomingEvent
        {
            Id = "id",
            AwayTeam = "awayTeam",
            HomeTeam = "homeTeam",
            CommenceTime = new DateTime(2026, 9, 22, 18, 0, 0, DateTimeKind.Utc),
            Odds = [ValidModels.CreateOdd()]
        };

        var predictorStub = Substitute.For<IOutcomePredictor>();
        predictorStub.GetOutcome(Arg.Any<IReadOnlyCollection<Odd>>()).Returns(OutcomeType.HomeTeam);

        var strategy = new PredictionStrategy(predictorStub);

        // Act
        var prediction = strategy.GetPrediction(upcomingEvent);

        // Assert
        prediction.Should().BeEquivalentTo(new EventPrediction
        {
            Id = upcomingEvent.Id,
            AwayTeam = upcomingEvent.AwayTeam,
            HomeTeam = upcomingEvent.HomeTeam,
            CommenceTime = upcomingEvent.CommenceTime,
            Outcome = OutcomeType.HomeTeam
        });
    }

    [Test]
    public void GetPrediction_WithNullUpcomingEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var strategy = new PredictionStrategy(Substitute.For<IOutcomePredictor>());

        // Act
        var action = () => strategy.GetPrediction(null!);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("upcomingEvent");
    }
}
