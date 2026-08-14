using OddsCollector.Functions.Models;
using FunctionApp = OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.Tests.Tests.Predictions;

internal sealed class OutcomeFinder
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

        var finder = new FunctionApp.OutcomeFinder(calculatorStub);

        // Act
        var outcome = finder.GetOutcome([new Odd()]);

        // Assert
        outcome.Should().NotBeNullOrEmpty().And.Be(expectedOutcome);
    }

    [Test]
    public void GetOutcome_WithNoOdds_ThrowsException()
    {
        var finder = new FunctionApp.OutcomeFinder(null!);

        var action = () => finder.GetOutcome([]);

        action.Should().Throw<ArgumentException>().WithParameterName("odds");
    }
}
