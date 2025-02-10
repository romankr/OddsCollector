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

        action.Should().Throw<ArgumentException>().WithParameterName("odds");
    }
}
