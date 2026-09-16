using FluentAssertions.Execution;
using OddsCollector.Functions.Models;
using FunctionApp = OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.Tests.Tests.Predictions;

internal sealed class ScoreCalculator
{
    [TestCase(0, TestName = "GetScores_WithZeroOdds_ScoresNothing")]
    [TestCase(1e-300, TestName = "GetScores_WithNearZeroOdds_ScoresNothing")]
    [TestCase(0.5, TestName = "GetScores_WithOddsBelowOne_ScoresNothing")]
    public void GetScores_WithUnusableOdds_ScoresNothing(double value)
    {
        List<Odd> odds = [new() { Away = value, Draw = value, Home = value }];

        var calculator = new FunctionApp.ScoreCalculator();

        // Act
        var scores = calculator.GetScores(odds);

        // Assert
        scores.Should().HaveCount(3).And.OnlyContain(s => s.Score == 0);
    }

    [Test]
    public void GetScores_WithOdds_ReturnsAdjustedConsensusScoresForDraw()
    {
        // Arrange
        List<Odd> odds =
        [
            new() { Away = 1, Draw = 2, Home = 3 },
            new() { Away = 3, Draw = 2, Home = 1 }
        ];

        var calculator = new FunctionApp.ScoreCalculator();

        // Act
        var scores = calculator.GetScores(odds);

        // Assert
        scores.Should().NotBeNullOrEmpty().And.HaveCount(3);

        var drawScore = scores[0];
        drawScore.Should().NotBeNull();

        using var scope = new AssertionScope();

        drawScore.Outcome.Should().NotBeNullOrEmpty().And.Be(OutcomeTypes.Draw);
        drawScore.Score.Should().BeApproximately(0.443, 0.001);
    }

    [Test]
    public void GetScores_WithOdds_ReturnsAdjustedConsensusScoresForAwayTeam()
    {
        // Arrange
        List<Odd> odds =
        [
            new() { Away = 1, Draw = 2, Home = 3 },
            new() { Away = 3, Draw = 2, Home = 1 }
        ];

        var calculator = new FunctionApp.ScoreCalculator();

        // Act
        var scores = calculator.GetScores(odds);

        // Assert
        scores.Should().NotBeNullOrEmpty().And.HaveCount(3);

        var awayScore = scores[1];
        awayScore.Should().NotBeNull();

        using var scope = new AssertionScope();

        awayScore.Outcome.Should().NotBeNullOrEmpty().And.Be(OutcomeTypes.AwayTeam);
        awayScore.Score.Should().BeApproximately(0.466, 0.001);
    }

    [Test]
    public void GetScores_WithOdds_ReturnsAdjustedConsensusScoresForHomeTeam()
    {
        // Arrange
        List<Odd> odds =
        [
            new() { Away = 1, Draw = 2, Home = 3 },
            new() { Away = 3, Draw = 2, Home = 1 }
        ];

        var calculator = new FunctionApp.ScoreCalculator();

        // Act
        var scores = calculator.GetScores(odds);

        // Assert
        scores.Should().NotBeNullOrEmpty().And.HaveCount(3);

        var homeScore = scores[2];
        homeScore.Should().NotBeNull();

        using var scope = new AssertionScope();

        homeScore.Outcome.Should().NotBeNullOrEmpty().And.Be(OutcomeTypes.HomeTeam);
        homeScore.Score.Should().BeApproximately(0.463, 0.001);
    }
}
