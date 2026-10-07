using FluentAssertions.Execution;
using OddsCollector.Functions.Models;
using FunctionApp = OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.Tests.Tests.Predictions;

/// <remarks>
///     A score is the mean implied probability (1 / decimal odds) of an outcome minus the adjustment
///     for it: 0.057 for a draw, 0.037 for an away win and 0.034 for a home win.
/// </remarks>
internal sealed class ScoreCalculator
{
    private const double Precision = 0.0001;

    // Draw: (1/2 + 1/2) / 2 - 0.057; away: (1/1 + 1/3) / 2 - 0.037; home: (1/3 + 1/1) / 2 - 0.034.
    [TestCase(OutcomeTypes.Draw, 0.443, TestName = "GetScores_WithOdds_ReturnsAdjustedConsensusScoreForDraw")]
    [TestCase(OutcomeTypes.AwayTeam, 0.6297,
        TestName = "GetScores_WithOdds_ReturnsAdjustedConsensusScoreForAwayTeam")]
    [TestCase(OutcomeTypes.HomeTeam, 0.6327,
        TestName = "GetScores_WithOdds_ReturnsAdjustedConsensusScoreForHomeTeam")]
    public void GetScores_WithOdds_ReturnsAdjustedConsensusScore(string outcome, double expectedScore)
    {
        // Arrange
        List<Odd> odds =
        [
            new() { Bookmaker = "bookmaker", Away = 1, Draw = 2, Home = 3 },
            new() { Bookmaker = "bookmaker", Away = 3, Draw = 2, Home = 1 }
        ];

        var calculator = new FunctionApp.ScoreCalculator();

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

        var calculator = new FunctionApp.ScoreCalculator();

        // Act
        var scores = calculator.GetScores(odds);

        // Assert: one bookmaker favouring the draw lifts it above both wins, which every bookmaker
        // prices the same. Draw: (1/1.5 + 1/6 + 1/6) / 3 - 0.057; away: 1/4.5 - 0.037; home: 1/4.5 - 0.034.
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

        var calculator = new FunctionApp.ScoreCalculator();

        // Act
        var scores = calculator.GetScores(odds);

        // Assert: (1/2 + 1/2) / 2 - 0.057, as if the zero quote were not there.
        scores.Should().ContainSingle(s => s.Outcome == OutcomeTypes.Draw)
            .Which.Score.Should().BeApproximately(0.443, Precision);
    }

    [TestCase(0, TestName = "GetScores_WithZeroOdds_ScoresNothing")]
    [TestCase(1e-300, TestName = "GetScores_WithNearZeroOdds_ScoresNothing")]
    [TestCase(0.5, TestName = "GetScores_WithOddsBelowOne_ScoresNothing")]
    public void GetScores_WithUnusableOdds_ScoresNothing(double value)
    {
        // Arrange
        List<Odd> odds = [new() { Bookmaker = "bookmaker", Away = value, Draw = value, Home = value }];

        var calculator = new FunctionApp.ScoreCalculator();

        // Act
        var scores = calculator.GetScores(odds);

        // Assert
        using var scope = new AssertionScope();

        scores.Select(s => s.Outcome).Should()
            .BeEquivalentTo(OutcomeTypes.Draw, OutcomeTypes.AwayTeam, OutcomeTypes.HomeTeam);
        scores.Should().OnlyContain(s => s.Score == 0);
    }
}
