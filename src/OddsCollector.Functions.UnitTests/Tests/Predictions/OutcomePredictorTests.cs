using FluentAssertions.Execution;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Predictions;
using OddsCollector.Functions.UnitTests.Infrastructure.Models;

namespace OddsCollector.Functions.UnitTests.Tests.Predictions;

internal sealed class OutcomePredictorTests
{
    [Test]
    public void TryGetOutcome_WithOdds_ReturnsOutcomeWithHighestScore()
    {
        // Arrange
        var predictor = CreatePredictor(
            new OutcomeScore { Outcome = OutcomeType.Draw, Score = 0.5 },
            new OutcomeScore { Outcome = OutcomeType.AwayTeam, Score = 1.0 },
            new OutcomeScore { Outcome = OutcomeType.HomeTeam, Score = 2.0 });

        // Act
        var predicted = predictor.TryGetOutcome([ValidModels.CreateOdd()], out var outcome);

        // Assert
        using var scope = new AssertionScope();

        predicted.Should().BeTrue();
        outcome.Should().Be(OutcomeType.HomeTeam);
    }

    [TestCase(OutcomeType.AwayTeam, OutcomeType.Draw, OutcomeType.HomeTeam,
        TestName = "TryGetOutcome_WithAllScoresEqual_ReturnsHomeTeam")]
    [TestCase(OutcomeType.HomeTeam, OutcomeType.AwayTeam, OutcomeType.Draw,
        TestName = "TryGetOutcome_WithAllScoresEqualInAnotherOrder_ReturnsHomeTeam")]
    public void TryGetOutcome_WithTiedScores_PrefersHomeTeam(OutcomeType first, OutcomeType second,
        OutcomeType third)
    {
        // Arrange
        var predictor = CreatePredictor(
            new OutcomeScore { Outcome = first, Score = 0.4 },
            new OutcomeScore { Outcome = second, Score = 0.4 },
            new OutcomeScore { Outcome = third, Score = 0.4 });

        // Act
        var predicted = predictor.TryGetOutcome([ValidModels.CreateOdd()], out var outcome);

        // Assert
        using var scope = new AssertionScope();

        predicted.Should().BeTrue();
        outcome.Should().Be(OutcomeType.HomeTeam);
    }

    [Test]
    public void TryGetOutcome_WithDrawAndAwayTeamTiedOnTop_ReturnsDraw()
    {
        // Arrange
        var predictor = CreatePredictor(
            new OutcomeScore { Outcome = OutcomeType.AwayTeam, Score = 0.4 },
            new OutcomeScore { Outcome = OutcomeType.Draw, Score = 0.4 },
            new OutcomeScore { Outcome = OutcomeType.HomeTeam, Score = 0.3 });

        // Act
        var predicted = predictor.TryGetOutcome([ValidModels.CreateOdd()], out var outcome);

        // Assert
        using var scope = new AssertionScope();

        predicted.Should().BeTrue();
        outcome.Should().Be(OutcomeType.Draw);
    }

    [Test]
    public void TryGetOutcome_WithNoOdds_ReturnsFalse()
    {
        // Arrange
        var calculatorMock = Substitute.For<IScoreCalculator>();

        var predictor = new OutcomePredictor(calculatorMock);

        // Act
        var predicted = predictor.TryGetOutcome([], out _);

        // Assert
        using var scope = new AssertionScope();

        predicted.Should().BeFalse();
        calculatorMock.ReceivedCalls().Should().BeEmpty();
    }

    [Test]
    public void TryGetOutcome_WithNullOdds_ThrowsArgumentNullException()
    {
        // Arrange
        var predictor = new OutcomePredictor(Substitute.For<IScoreCalculator>());

        // Act
        var action = () => predictor.TryGetOutcome(null!, out _);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("odds");
    }

    [TestCase(0, TestName = "TryGetOutcome_WithNothingScoring_ReturnsFalse")]
    [TestCase(-0.01, TestName = "TryGetOutcome_WithOnlyNegativeScores_ReturnsFalse")]
    public void TryGetOutcome_WithNoPositiveScore_ReturnsFalse(double score)
    {
        // Arrange
        var predictor = CreatePredictor(
            new OutcomeScore { Outcome = OutcomeType.Draw, Score = score },
            new OutcomeScore { Outcome = OutcomeType.AwayTeam, Score = score },
            new OutcomeScore { Outcome = OutcomeType.HomeTeam, Score = score });

        // Act
        var predicted = predictor.TryGetOutcome([ValidModels.CreateOdd()], out _);

        // Assert
        predicted.Should().BeFalse();
    }

    [Test]
    public void TryGetOutcome_WithUnusableOdds_ReturnsFalseInsteadOfPredictingADraw()
    {
        // Arrange
        var predictor = new OutcomePredictor(new ScoreCalculator());

        // Act
        var predicted = predictor.TryGetOutcome([ValidModels.CreateOdd(home: 0, draw: 0, away: 0)], out _);

        // Assert
        predicted.Should().BeFalse();
    }

    private static OutcomePredictor CreatePredictor(params OutcomeScore[] scores)
    {
        var calculatorStub = Substitute.For<IScoreCalculator>();
        calculatorStub.GetScores(Arg.Any<IReadOnlyCollection<Odd>>()).Returns(scores);

        return new OutcomePredictor(calculatorStub);
    }
}
