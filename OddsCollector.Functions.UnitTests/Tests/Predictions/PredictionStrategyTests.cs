using FluentAssertions.Execution;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Predictions;
using OddsCollector.Functions.UnitTests.Infrastructure.Models;

namespace OddsCollector.Functions.UnitTests.Tests.Predictions;

internal sealed class PredictionStrategyTests
{
    [Test]
    public void TryGetPrediction_WithPredictableEvent_ReturnsPredictionForIt()
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
        var predicted = strategy.TryGetPrediction(upcomingEvent, out var prediction);

        // Assert
        using var scope = new AssertionScope();

        predicted.Should().BeTrue();
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
    public void TryGetPrediction_WithUpcomingEvent_PassesItsOddsToThePredictor()
    {
        // Arrange
        var upcomingEvent = ValidModels.CreateUpcomingEvent();

        var predictorMock = CreatePredictorStub(OutcomeType.HomeTeam);

        var strategy = new PredictionStrategy(predictorMock);

        // Act
        strategy.TryGetPrediction(upcomingEvent, out _);

        // Assert: the event's own list is handed over as is, without being copied first.
        predictorMock.Received(1).TryGetOutcome(upcomingEvent.Odds, out Arg.Any<OutcomeType>());
    }

    [Test]
    public void TryGetPrediction_WithEventNothingCanBePredictedFrom_ReturnsFalse()
    {
        // Arrange
        var predictorStub = Substitute.For<IOutcomePredictor>();
        predictorStub.TryGetOutcome(Arg.Any<IReadOnlyCollection<Odd>>(), out Arg.Any<OutcomeType>()).Returns(false);

        var strategy = new PredictionStrategy(predictorStub);

        // Act
        var predicted = strategy.TryGetPrediction(ValidModels.CreateUpcomingEvent(), out var prediction);

        // Assert
        using var scope = new AssertionScope();

        predicted.Should().BeFalse();
        prediction.Should().BeNull();
    }

    [Test]
    public void TryGetPrediction_WithNullUpcomingEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var strategy = new PredictionStrategy(Substitute.For<IOutcomePredictor>());

        // Act
        var action = () => strategy.TryGetPrediction(null!, out _);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("upcomingEvent");
    }

    private static IOutcomePredictor CreatePredictorStub(OutcomeType outcome)
    {
        var predictorStub = Substitute.For<IOutcomePredictor>();
        predictorStub.TryGetOutcome(Arg.Any<IReadOnlyCollection<Odd>>(), out Arg.Any<OutcomeType>())
            .Returns(call =>
            {
                call[1] = outcome;

                return true;
            });

        return predictorStub;
    }
}
