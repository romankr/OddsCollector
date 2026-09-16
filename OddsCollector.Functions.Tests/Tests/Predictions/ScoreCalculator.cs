using FluentAssertions.Execution;
using OddsCollector.Functions.Models;
using FunctionApp = OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.Tests.Tests.Predictions;

internal sealed class ScoreCalculator
{
    [Test]
    public void GetScores_WithBookmakersDisagreeing_RanksOnTheMeanOfWhatEachImplies()
    {
        // Arrange: one book quotes the draw at 1.5 and two at 6.0. Inverting the mean of the
        // odds scores the draw 0.165 and hands the prediction to the away team; the mean of
        // what each book implies scores it 0.276 and hands it to the draw.
        List<Odd> odds =
        [
            new() { Home = 4.5, Draw = 1.5, Away = 4.5 },
            new() { Home = 4.5, Draw = 6.0, Away = 4.5 },
            new() { Home = 4.5, Draw = 6.0, Away = 4.5 }
        ];

        var calculator = new FunctionApp.ScoreCalculator();

        // Act
        var scores = calculator.GetScores(odds);

        // Assert
        using var scope = new AssertionScope();

        scores[0].Outcome.Should().Be(OutcomeTypes.Draw);
        scores[0].Score.Should().BeApproximately(0.2763, 0.0001);
        scores[1].Score.Should().BeApproximately(0.1882, 0.0001);
        scores[2].Score.Should().BeApproximately(0.1852, 0.0001);

        scores.MaxBy(s => s.Score)!.Outcome.Should().Be(OutcomeTypes.Draw);
    }

    [Test]
    public void GetScores_WithOneUnusableQuote_LeavesItOutOfTheConsensus()
    {
        // Arrange: the middle book quotes nothing usable for the draw. Dropping it has to
        // leave the consensus of the other two untouched, rather than averaging in a zero.
        List<Odd> odds =
        [
            new() { Home = 2, Draw = 2, Away = 2 },
            new() { Home = 2, Draw = 0, Away = 2 },
            new() { Home = 2, Draw = 2, Away = 2 }
        ];

        var calculator = new FunctionApp.ScoreCalculator();

        // Act
        var scores = calculator.GetScores(odds);

        // Assert
        using var scope = new AssertionScope();

        scores[0].Outcome.Should().Be(OutcomeTypes.Draw);
        scores[0].Score.Should().BeApproximately(0.443, 0.0001);
    }

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
        drawScore.Score.Should().BeApproximately(0.443, 0.0001);
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
        awayScore.Score.Should().BeApproximately(0.6327, 0.0001);
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
        homeScore.Score.Should().BeApproximately(0.6297, 0.0001);
    }
}
