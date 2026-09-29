using OddsCollector.Functions.Models;
using FunctionApp = OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.Tests.Tests.Predictions;

internal sealed class OutcomePredictor
{
    [Test]
    public void GetOutcome_WithOdds_ReturnsOutcome()
    {
        // Arrange
        const string expectedOutcome = nameof(expectedOutcome);

        var calculatorStub = Substitute.For<FunctionApp.IScoreCalculator>();
        calculatorStub.GetScores(Arg.Any<ICollection<Odd>>()).Returns(
            [
                new FunctionApp.OutcomeScore { Outcome = expectedOutcome, Score = 2.0 },
                new FunctionApp.OutcomeScore { Outcome = "loser", Score = 1.0 },
                new FunctionApp.OutcomeScore { Outcome = "draw", Score = 0.5 }
            ]
        );

        var finder = new FunctionApp.OutcomePredictor(calculatorStub);

        // Act
        var outcome = finder.GetOutcome([new Odd()]);

        // Assert
        outcome.Should().NotBeNullOrEmpty().And.Be(expectedOutcome);
    }

    [Test]
    public void GetOutcome_WithNoOdds_ThrowsException()
    {
        var finder = new FunctionApp.OutcomePredictor(null!);

        var action = () => finder.GetOutcome([]);

        action.Should().Throw<ArgumentException>().WithParameterName("odds")
            .WithMessage("odds cannot be empty*");
    }

    [Test]
    public void GetOutcome_WithNothingScoring_ThrowsException()
    {
        var calculatorStub = Substitute.For<FunctionApp.IScoreCalculator>();
        calculatorStub.GetScores(Arg.Any<ICollection<Odd>>()).Returns(
            [
                new FunctionApp.OutcomeScore { Outcome = OutcomeTypes.Draw, Score = 0 },
                new FunctionApp.OutcomeScore { Outcome = OutcomeTypes.AwayTeam, Score = 0 },
                new FunctionApp.OutcomeScore { Outcome = OutcomeTypes.HomeTeam, Score = 0 }
            ]
        );

        var finder = new FunctionApp.OutcomePredictor(calculatorStub);

        var action = () => finder.GetOutcome([new Odd()]);

        action.Should().Throw<ArgumentException>().WithParameterName("odds")
            .WithMessage("odds has no usable quote*");
    }

    [Test]
    public void GetOutcome_WithUnusableOdds_ThrowsInsteadOfPredictingADraw()
    {
        var finder = new FunctionApp.OutcomePredictor(new FunctionApp.ScoreCalculator());

        var action = () => finder.GetOutcome([new Odd { Home = 0, Draw = 0, Away = 0 }]);

        action.Should().Throw<ArgumentException>().WithParameterName("odds")
            .WithMessage("odds has no usable quote*");
    }
}
