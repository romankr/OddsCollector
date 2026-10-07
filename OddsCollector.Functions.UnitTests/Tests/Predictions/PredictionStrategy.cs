using OddsCollector.Functions.Models;
using OddsCollector.Tests.Infrastructure.Models;
using FunctionApp = OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.Tests.Tests.Predictions;

internal sealed class PredictionStrategy
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

        var predictorStub = Substitute.For<FunctionApp.IOutcomePredictor>();
        predictorStub.GetOutcome(Arg.Any<ICollection<Odd>>()).Returns(OutcomeTypes.HomeTeam);

        var strategy = new FunctionApp.PredictionStrategy(predictorStub);

        // Act
        var prediction = strategy.GetPrediction(upcomingEvent);

        // Assert
        prediction.Should().BeEquivalentTo(new EventPrediction
        {
            Id = upcomingEvent.Id,
            AwayTeam = upcomingEvent.AwayTeam,
            HomeTeam = upcomingEvent.HomeTeam,
            CommenceTime = upcomingEvent.CommenceTime,
            Outcome = OutcomeTypes.HomeTeam
        });
    }

    [Test]
    public void GetPrediction_WithNullUpcomingEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var strategy = new FunctionApp.PredictionStrategy(Substitute.For<FunctionApp.IOutcomePredictor>());

        // Act
        var action = () => strategy.GetPrediction(null);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("upcomingEvent");
    }
}
