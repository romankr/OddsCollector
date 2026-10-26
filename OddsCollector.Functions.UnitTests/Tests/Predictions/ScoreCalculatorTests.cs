using FluentAssertions.Execution;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.UnitTests.Tests.Predictions;

internal sealed class ScoreCalculatorTests
{
    private const double Precision = 0.0001;

    [TestCase(OutcomeTypes.Draw, 0.443, TestName = "GetScores_WithOdds_ReturnsAdjustedConsensusScoreForDraw")]
    [TestCase(OutcomeTypes.AwayTeam, 0.5297,
        TestName = "GetScores_WithOdds_ReturnsAdjustedConsensusScoreForAwayTeam")]
    [TestCase(OutcomeTypes.HomeTeam, 0.5327,
        TestName = "GetScores_WithOdds_ReturnsAdjustedConsensusScoreForHomeTeam")]
    public void GetScores_WithOdds_ReturnsAdjustedConsensusScore(string outcome, double expectedScore)
    {
        // Arrange
        List<Odd> odds =
        [
            new() { Bookmaker = "bookmaker", Away = 1.25, Draw = 2, Home = 3 },
            new() { Bookmaker = "bookmaker", Away = 3, Draw = 2, Home = 1.25 }
        ];

        var calculator = new ScoreCalculator();

        // Act
        var scores = calculator.GetScores(odds);

        // Assert
        scores.Should().ContainSingle(s => s.Outcome == outcome)
            .Which.Score.Should().BeApproximately(expectedScore, Precision);
    }

    [Test]
    public void GetScores_WithBookmakersDisagreeing_RanksOnTheMeanOfWhatEachImplies()
    {
        // Arrange
        List<Odd> odds =
        [
            new() { Bookmaker = "bookmaker", Home = 4.5, Draw = 1.5, Away = 4.5 },
            new() { Bookmaker = "bookmaker", Home = 4.5, Draw = 6.0, Away = 4.5 },
            new() { Bookmaker = "bookmaker", Home = 4.5, Draw = 6.0, Away = 4.5 }
        ];

        var calculator = new ScoreCalculator();

        // Act
        var scores = calculator.GetScores(odds);

        // Assert
        scores.Should().BeEquivalentTo(
        [
            new { Outcome = OutcomeTypes.Draw, Score = 0.2763 },
            new { Outcome = OutcomeTypes.AwayTeam, Score = 0.1852 },
            new { Outcome = OutcomeTypes.HomeTeam, Score = 0.1882 }
        ], options => options.Using<double>(c => c.Subject.Should().BeApproximately(c.Expectation, Precision))
            .WhenTypeIs<double>());
    }

    [Test]
    public void GetScores_WithOneUnusableQuote_LeavesItOutOfTheConsensus()
    {
        // Arrange
        List<Odd> odds =
        [
            new() { Bookmaker = "bookmaker", Home = 2, Draw = 2, Away = 2 },
            new() { Bookmaker = "bookmaker", Home = 2, Draw = 0, Away = 2 },
            new() { Bookmaker = "bookmaker", Home = 2, Draw = 2, Away = 2 }
        ];

        var calculator = new ScoreCalculator();

        // Act
        var scores = calculator.GetScores(odds);

        // Assert
        scores.Should().ContainSingle(s => s.Outcome == OutcomeTypes.Draw)
            .Which.Score.Should().BeApproximately(0.443, Precision);
    }

    [TestCase(0, TestName = "GetScores_WithZeroOdds_ScoresNothing")]
    [TestCase(1e-300, TestName = "GetScores_WithNearZeroOdds_ScoresNothing")]
    [TestCase(0.5, TestName = "GetScores_WithOddsBelowOne_ScoresNothing")]
    [TestCase(1, TestName = "GetScores_WithOddsOfOne_ScoresNothing")]
    public void GetScores_WithUnusableOdds_ScoresNothing(double value)
    {
        // Arrange
        List<Odd> odds = [new() { Bookmaker = "bookmaker", Away = value, Draw = value, Home = value }];

        var calculator = new ScoreCalculator();

        // Act
        var scores = calculator.GetScores(odds);

        // Assert
        using var scope = new AssertionScope();

        scores.Select(s => s.Outcome).Should()
            .BeEquivalentTo(OutcomeTypes.Draw, OutcomeTypes.AwayTeam, OutcomeTypes.HomeTeam);
        scores.Should().OnlyContain(s => s.Score == 0);
    }
}
