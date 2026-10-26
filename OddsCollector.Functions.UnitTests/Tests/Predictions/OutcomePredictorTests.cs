using OddsCollector.Functions.Models;
using OddsCollector.Functions.Predictions;
using OddsCollector.Functions.UnitTests.Infrastructure.Models;

namespace OddsCollector.Functions.UnitTests.Tests.Predictions;

internal sealed class OutcomePredictorTests
{
    [Test]
    public void GetOutcome_WithOdds_ReturnsOutcomeWithHighestScore()
    {
        // Arrange
        var calculatorStub = Substitute.For<IScoreCalculator>();
        calculatorStub.GetScores(Arg.Any<IReadOnlyCollection<Odd>>()).Returns(
        [
            new OutcomeScore { Outcome = OutcomeType.Draw, Score = 0.5 },
            new OutcomeScore { Outcome = OutcomeType.AwayTeam, Score = 1.0 },
            new OutcomeScore { Outcome = OutcomeType.HomeTeam, Score = 2.0 }
        ]);

        var predictor = new OutcomePredictor(calculatorStub);

        // Act
        var outcome = predictor.GetOutcome([ValidModels.CreateOdd()]);

        // Assert
        outcome.Should().Be(OutcomeType.HomeTeam);
    }

    [TestCase(OutcomeType.AwayTeam, OutcomeType.Draw, OutcomeType.HomeTeam,
        TestName = "GetOutcome_WithAllScoresEqual_ReturnsHomeTeam")]
    [TestCase(OutcomeType.HomeTeam, OutcomeType.AwayTeam, OutcomeType.Draw,
        TestName = "GetOutcome_WithAllScoresEqualInAnotherOrder_ReturnsHomeTeam")]
    public void GetOutcome_WithTiedScores_PrefersHomeTeam(OutcomeType first, OutcomeType second, OutcomeType third)
    {
        // Arrange
        var calculatorStub = Substitute.For<IScoreCalculator>();
        calculatorStub.GetScores(Arg.Any<IReadOnlyCollection<Odd>>()).Returns(
        [
            new OutcomeScore { Outcome = first, Score = 0.4 },
            new OutcomeScore { Outcome = second, Score = 0.4 },
            new OutcomeScore { Outcome = third, Score = 0.4 }
        ]);

        var predictor = new OutcomePredictor(calculatorStub);

        // Act
        var outcome = predictor.GetOutcome([ValidModels.CreateOdd()]);

        // Assert
        outcome.Should().Be(OutcomeType.HomeTeam);
    }

    [Test]
    public void GetOutcome_WithDrawAndAwayTeamTiedOnTop_ReturnsDraw()
    {
        // Arrange
        var calculatorStub = Substitute.For<IScoreCalculator>();
        calculatorStub.GetScores(Arg.Any<IReadOnlyCollection<Odd>>()).Returns(
        [
            new OutcomeScore { Outcome = OutcomeType.AwayTeam, Score = 0.4 },
            new OutcomeScore { Outcome = OutcomeType.Draw, Score = 0.4 },
            new OutcomeScore { Outcome = OutcomeType.HomeTeam, Score = 0.3 }
        ]);

        var predictor = new OutcomePredictor(calculatorStub);

        // Act
        var outcome = predictor.GetOutcome([ValidModels.CreateOdd()]);

        // Assert
        outcome.Should().Be(OutcomeType.Draw);
    }

    [Test]
    public void GetOutcome_WithNoOdds_ThrowsArgumentException()
    {
        // Arrange
        var predictor = new OutcomePredictor(Substitute.For<IScoreCalculator>());

        // Act
        var action = () => predictor.GetOutcome([]);

        // Assert
        action.Should().Throw<ArgumentException>().WithParameterName("odds")
            .WithMessage("odds cannot be empty*");
    }

    [Test]
    public void GetOutcome_WithNullOdds_ThrowsArgumentNullException()
    {
        // Arrange
        var predictor = new OutcomePredictor(Substitute.For<IScoreCalculator>());

        // Act
        var action = () => predictor.GetOutcome(null!);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("odds");
    }

    [Test]
    public void GetOutcome_WithNothingScoring_ThrowsArgumentException()
    {
        // Arrange
        var calculatorStub = Substitute.For<IScoreCalculator>();
        calculatorStub.GetScores(Arg.Any<IReadOnlyCollection<Odd>>()).Returns(
        [
            new OutcomeScore { Outcome = OutcomeType.Draw, Score = 0 },
            new OutcomeScore { Outcome = OutcomeType.AwayTeam, Score = 0 },
            new OutcomeScore { Outcome = OutcomeType.HomeTeam, Score = 0 }
        ]);

        var predictor = new OutcomePredictor(calculatorStub);

        // Act
        var action = () => predictor.GetOutcome([ValidModels.CreateOdd()]);

        // Assert
        action.Should().Throw<ArgumentException>().WithParameterName("odds")
            .WithMessage("odds has no usable quote*");
    }

    [Test]
    public void GetOutcome_WithUnusableOdds_ThrowsInsteadOfPredictingADraw()
    {
        // Arrange
        var predictor = new OutcomePredictor(new ScoreCalculator());

        // Act
        var action = () => predictor.GetOutcome([ValidModels.CreateOdd(home: 0, draw: 0, away: 0)]);

        // Assert
        action.Should().Throw<ArgumentException>().WithParameterName("odds")
            .WithMessage("odds has no usable quote*");
    }
}
