using FluentAssertions.Execution;
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
        var predictor = CreatePredictor(
            new OutcomeScore { Outcome = OutcomeType.Draw, Score = 0.5 },
            new OutcomeScore { Outcome = OutcomeType.AwayTeam, Score = 1.0 },
            new OutcomeScore { Outcome = OutcomeType.HomeTeam, Score = 2.0 });

        // Act
        var result = predictor.GetOutcome([ValidModels.CreateOdd()]);

        // Assert
        result.Value.Should().Be(OutcomeType.HomeTeam);
    }

    [TestCase(OutcomeType.AwayTeam, OutcomeType.Draw, OutcomeType.HomeTeam,
        TestName = "GetOutcome_WithAllScoresEqual_ReturnsHomeTeam")]
    [TestCase(OutcomeType.HomeTeam, OutcomeType.AwayTeam, OutcomeType.Draw,
        TestName = "GetOutcome_WithAllScoresEqualInAnotherOrder_ReturnsHomeTeam")]
    public void GetOutcome_WithTiedScores_PrefersHomeTeam(OutcomeType first, OutcomeType second,
        OutcomeType third)
    {
        // Arrange
        var predictor = CreatePredictor(
            new OutcomeScore { Outcome = first, Score = 0.4 },
            new OutcomeScore { Outcome = second, Score = 0.4 },
            new OutcomeScore { Outcome = third, Score = 0.4 });

        // Act
        var result = predictor.GetOutcome([ValidModels.CreateOdd()]);

        // Assert
        result.Value.Should().Be(OutcomeType.HomeTeam);
    }

    [Test]
    public void GetOutcome_WithDrawAndAwayTeamTiedOnTop_ReturnsDraw()
    {
        // Arrange
        var predictor = CreatePredictor(
            new OutcomeScore { Outcome = OutcomeType.AwayTeam, Score = 0.4 },
            new OutcomeScore { Outcome = OutcomeType.Draw, Score = 0.4 },
            new OutcomeScore { Outcome = OutcomeType.HomeTeam, Score = 0.3 });

        // Act
        var result = predictor.GetOutcome([ValidModels.CreateOdd()]);

        // Assert
        result.Value.Should().Be(OutcomeType.Draw);
    }

    [Test]
    public void GetOutcome_WithNoOdds_ReturnsNoUsableOdds()
    {
        // Arrange
        var calculatorMock = Substitute.For<IScoreCalculator>();

        var predictor = new OutcomePredictor(calculatorMock);

        // Act
        var result = predictor.GetOutcome([]);

        // Assert
        using var scope = new AssertionScope();

        result.Value.Should().BeOfType<NoUsableOdds>();
        calculatorMock.ReceivedCalls().Should().BeEmpty();
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

    [TestCase(0, TestName = "GetOutcome_WithNothingScoring_ReturnsNoUsableOdds")]
    [TestCase(-0.01, TestName = "GetOutcome_WithOnlyNegativeScores_ReturnsNoUsableOdds")]
    public void GetOutcome_WithNoPositiveScore_ReturnsNoUsableOdds(double score)
    {
        // Arrange
        var predictor = CreatePredictor(
            new OutcomeScore { Outcome = OutcomeType.Draw, Score = score },
            new OutcomeScore { Outcome = OutcomeType.AwayTeam, Score = score },
            new OutcomeScore { Outcome = OutcomeType.HomeTeam, Score = score });

        // Act
        var result = predictor.GetOutcome([ValidModels.CreateOdd()]);

        // Assert
        result.Value.Should().BeOfType<NoUsableOdds>();
    }

    [Test]
    public void GetOutcome_WithUnusableOdds_ReturnsNoUsableOddsInsteadOfPredictingADraw()
    {
        // Arrange
        var predictor = new OutcomePredictor(new ScoreCalculator());

        // Act
        var result = predictor.GetOutcome([ValidModels.CreateOdd(home: 0, draw: 0, away: 0)]);

        // Assert
        result.Value.Should().BeOfType<NoUsableOdds>();
    }

    private static OutcomePredictor CreatePredictor(params OutcomeScore[] scores)
    {
        var calculatorStub = Substitute.For<IScoreCalculator>();
        calculatorStub.GetScores(Arg.Any<IReadOnlyCollection<Odd>>()).Returns(scores);

        return new OutcomePredictor(calculatorStub);
    }
}
