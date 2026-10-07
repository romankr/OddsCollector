using OddsCollector.Functions.Models;
using OddsCollector.Tests.Infrastructure.Models;
using FunctionApp = OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.Tests.Tests.Predictions;

internal sealed class OutcomePredictor
{
    [Test]
    public void GetOutcome_WithOdds_ReturnsOutcomeWithHighestScore()
    {
        // Arrange
        var calculatorStub = Substitute.For<FunctionApp.IScoreCalculator>();
        calculatorStub.GetScores(Arg.Any<ICollection<Odd>>()).Returns(
        [
            new FunctionApp.OutcomeScore { Outcome = OutcomeTypes.Draw, Score = 0.5 },
            new FunctionApp.OutcomeScore { Outcome = OutcomeTypes.AwayTeam, Score = 1.0 },
            new FunctionApp.OutcomeScore { Outcome = OutcomeTypes.HomeTeam, Score = 2.0 }
        ]);

        var predictor = new FunctionApp.OutcomePredictor(calculatorStub);

        // Act
        var outcome = predictor.GetOutcome([ValidModels.CreateOdd()]);

        // Assert
        outcome.Should().Be(OutcomeTypes.HomeTeam);
    }

    [Test]
    public void GetOutcome_WithNoOdds_ThrowsArgumentException()
    {
        // Arrange
        var predictor = new FunctionApp.OutcomePredictor(Substitute.For<FunctionApp.IScoreCalculator>());

        // Act
        var action = () => predictor.GetOutcome([]);

        // Assert
        action.Should().Throw<ArgumentException>().WithParameterName("odds")
            .WithMessage("odds cannot be empty*");
    }

    [Test]
    public void GetOutcome_WithNothingScoring_ThrowsArgumentException()
    {
        // Arrange
        var calculatorStub = Substitute.For<FunctionApp.IScoreCalculator>();
        calculatorStub.GetScores(Arg.Any<ICollection<Odd>>()).Returns(
        [
            new FunctionApp.OutcomeScore { Outcome = OutcomeTypes.Draw, Score = 0 },
            new FunctionApp.OutcomeScore { Outcome = OutcomeTypes.AwayTeam, Score = 0 },
            new FunctionApp.OutcomeScore { Outcome = OutcomeTypes.HomeTeam, Score = 0 }
        ]);

        var predictor = new FunctionApp.OutcomePredictor(calculatorStub);

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
        var predictor = new FunctionApp.OutcomePredictor(new FunctionApp.ScoreCalculator());

        // Act
        var action = () => predictor.GetOutcome([ValidModels.CreateOdd(home: 0, draw: 0, away: 0)]);

        // Assert
        action.Should().Throw<ArgumentException>().WithParameterName("odds")
            .WithMessage("odds has no usable quote*");
    }
}
