using OddsCollector.Functions.Models;
using FunctionApp = OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.Tests.Tests.Predictions;

internal sealed class WinnerFinder
{
    [Test]
    public void GetWinner_WithOdds_ReturnsWinner()
    {
        // Arrange
        const string expectedWinner = nameof(expectedWinner);

        var calculatorStub = Substitute.For<FunctionApp.IScoreCalculator>();
        calculatorStub.GetScores(Arg.Any<ICollection<Odd>>()).Returns(
            [
                new FunctionApp.OutcomeScore { Outcome = expectedWinner, Score = 2.0 },
                new FunctionApp.OutcomeScore { Outcome = "loser", Score = 1.0 },
                new FunctionApp.OutcomeScore { Outcome = "draw", Score = 0.5 }
            ]
        );

        var finder = new FunctionApp.WinnerFinder(calculatorStub);

        // Act
        var winner = finder.GetWinner([new Odd()]);

        // Assert
        winner.Should().NotBeNullOrEmpty().And.Be(expectedWinner);
    }

    [Test]
    public void GetWinner_WithNoOdds_ThrowsException()
    {
        var finder = new FunctionApp.WinnerFinder(null!);

        var action = () => finder.GetWinner([]);

        action.Should().Throw<ArgumentException>().WithParameterName("odds")
            .WithMessage("odds cannot be empty*");
    }

    [Test]
    public void GetWinner_WithNothingScoring_ThrowsException()
    {
        // Every outcome at the same zero is what the calculator returns when no bookmaker
        // quoted anything usable. MaxBy would pick the first of them.
        var calculatorStub = Substitute.For<FunctionApp.IScoreCalculator>();
        calculatorStub.GetScores(Arg.Any<ICollection<Odd>>()).Returns(
            [
                new FunctionApp.OutcomeScore { Outcome = OutcomeTypes.Draw, Score = 0 },
                new FunctionApp.OutcomeScore { Outcome = OutcomeTypes.AwayTeam, Score = 0 },
                new FunctionApp.OutcomeScore { Outcome = OutcomeTypes.HomeTeam, Score = 0 }
            ]
        );

        var finder = new FunctionApp.WinnerFinder(calculatorStub);

        var action = () => finder.GetWinner([new Odd()]);

        action.Should().Throw<ArgumentException>().WithParameterName("odds")
            .WithMessage("odds has no usable quote*");
    }

    [Test]
    public void GetWinner_WithUnusableOdds_ThrowsInsteadOfPredictingADraw()
    {
        // The real calculator, so that this pins the whole path: quotes below the minimum
        // decimal odd score nothing, and nothing must not become a prediction.
        var finder = new FunctionApp.WinnerFinder(new FunctionApp.ScoreCalculator());

        var action = () => finder.GetWinner([new Odd { Home = 0, Draw = 0, Away = 0 }]);

        action.Should().Throw<ArgumentException>().WithParameterName("odds")
            .WithMessage("odds has no usable quote*");
    }
}
