using OddsCollector.Functions.Models;
using OddsCollector.Functions.Predictions;
using OddsCollector.Functions.UnitTests.Infrastructure.Models;

namespace OddsCollector.Functions.UnitTests.Tests.Predictions;

internal sealed class PredictionStrategyTests
{
    [Test]
    public void GetPrediction_WithPredictableEvent_ReturnsPredictionForIt()
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

        var strategy = new PredictionStrategy(CreatePredictorStub(OutcomeType.HomeTeam));

        // Act
        var result = strategy.GetPrediction(upcomingEvent);

        // Assert
        result.Value.Should().BeEquivalentTo(new EventPrediction
        {
            Id = upcomingEvent.Id,
            AwayTeam = upcomingEvent.AwayTeam,
            HomeTeam = upcomingEvent.HomeTeam,
            CommenceTime = upcomingEvent.CommenceTime,
            Outcome = OutcomeType.HomeTeam
        });
    }

    [Test]
    public void GetPrediction_WithUpcomingEvent_PassesItsOddsToThePredictor()
    {
        // Arrange
        var upcomingEvent = ValidModels.CreateUpcomingEvent();

        var predictorMock = CreatePredictorStub(OutcomeType.HomeTeam);

        var strategy = new PredictionStrategy(predictorMock);

        // Act
        strategy.GetPrediction(upcomingEvent);

        // Assert
        predictorMock.Received(1).GetOutcome(upcomingEvent.Odds);
    }

    [Test]
    public void GetPrediction_WithEventNothingCanBePredictedFrom_ReturnsNoUsableOdds()
    {
        // Arrange
        OutcomeResult noUsableOdds = new NoUsableOdds();

        var predictorStub = Substitute.For<IOutcomePredictor>();
        predictorStub.GetOutcome(Arg.Any<IReadOnlyCollection<Odd>>()).Returns(noUsableOdds);

        var strategy = new PredictionStrategy(predictorStub);

        // Act
        var result = strategy.GetPrediction(ValidModels.CreateUpcomingEvent());

        // Assert
        result.Value.Should().BeOfType<NoUsableOdds>();
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

    private static IOutcomePredictor CreatePredictorStub(OutcomeType outcome)
    {
        OutcomeResult result = outcome;

        var predictorStub = Substitute.For<IOutcomePredictor>();
        predictorStub.GetOutcome(Arg.Any<IReadOnlyCollection<Odd>>()).Returns(result);

        return predictorStub;
    }
}
