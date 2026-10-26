using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.UnitTests.Tests.OddsApi.Converters;

internal sealed class OutcomeConverterTests
{
    private const string AwayTeam = "awayTeam";
    private const string HomeTeam = "homeTeam";

    [TestCase(1, 1, OutcomeType.Draw, TestName = "GetOutcome_WithEqualScores_ReturnsDraw")]
    [TestCase(2, 1, OutcomeType.HomeTeam, TestName = "GetOutcome_WithHomeTeamScoringMore_ReturnsHomeTeam")]
    [TestCase(1, 2, OutcomeType.AwayTeam, TestName = "GetOutcome_WithAwayTeamScoringMore_ReturnsAwayTeam")]
    public void GetOutcome_WithScores_ReturnsWinnerSideOrDraw(int homeScore, int awayScore,
        OutcomeType expectedOutcome)
    {
        // Arrange
        var converter = CreateConverter(
            new EventScore { Name = HomeTeam, Score = homeScore },
            new EventScore { Name = AwayTeam, Score = awayScore });

        // Act
        var outcome = converter.GetOutcome([], AwayTeam, HomeTeam);

        // Assert
        outcome.Should().Be(expectedOutcome);
    }

    [Test]
    public void GetOutcome_WithAwayTeamListedFirst_MatchesScoresByTeamName()
    {
        // Arrange: the API does not promise any order of the two scores.
        var converter = CreateConverter(
            new EventScore { Name = AwayTeam, Score = 0 },
            new EventScore { Name = HomeTeam, Score = 3 });

        // Act
        var outcome = converter.GetOutcome([], AwayTeam, HomeTeam);

        // Assert
        outcome.Should().Be(OutcomeType.HomeTeam);
    }

    [TestCase(HomeTeam, "otherTeam", AwayTeam,
        TestName = "GetOutcome_WithoutAwayTeamScore_ThrowsArgumentExceptionNamingIt")]
    [TestCase("otherTeam", AwayTeam, HomeTeam,
        TestName = "GetOutcome_WithoutHomeTeamScore_ThrowsArgumentExceptionNamingIt")]
    public void GetOutcome_WithoutTeamScore_ThrowsArgumentExceptionNamingIt(string firstScoredTeam,
        string secondScoredTeam, string missingTeam)
    {
        // Arrange
        var converter = CreateConverter(
            new EventScore { Name = firstScoredTeam, Score = 1 },
            new EventScore { Name = secondScoredTeam, Score = 0 });

        // Act
        var action = () => converter.GetOutcome([], AwayTeam, HomeTeam);

        // Assert
        action.Should().ThrowExactly<ArgumentException>()
            .WithMessage($"Score of {missingTeam} is missing*").And.ParamName.Should().Be("scores");
    }

    [Test]
    public void GetOutcome_WithNullScores_ThrowsArgumentNullException()
    {
        // Arrange
        var converter = CreateConverter();

        // Act
        var action = () => converter.GetOutcome(null, AwayTeam, HomeTeam);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("scores");
    }

    [TestCase(null, TestName = "GetOutcome_WithNullAwayTeam_ThrowsArgumentException")]
    [TestCase(" ", TestName = "GetOutcome_WithWhitespaceAwayTeam_ThrowsArgumentException")]
    public void GetOutcome_WithBlankAwayTeam_ThrowsArgumentException(string? awayTeam)
    {
        // Arrange
        var converter = CreateConverter();

        // Act
        var action = () => converter.GetOutcome([], awayTeam, HomeTeam);

        // Assert
        action.Should().Throw<ArgumentException>().WithParameterName(nameof(awayTeam));
    }

    [TestCase(null, TestName = "GetOutcome_WithNullHomeTeam_ThrowsArgumentException")]
    [TestCase(" ", TestName = "GetOutcome_WithWhitespaceHomeTeam_ThrowsArgumentException")]
    public void GetOutcome_WithBlankHomeTeam_ThrowsArgumentException(string? homeTeam)
    {
        // Arrange
        var converter = CreateConverter();

        // Act
        var action = () => converter.GetOutcome([], AwayTeam, homeTeam);

        // Assert
        action.Should().Throw<ArgumentException>().WithParameterName(nameof(homeTeam));
    }

    private static OutcomeConverter CreateConverter(params EventScore[] eventScores)
    {
        var scoreModelsConverterStub = Substitute.For<IScoreModelsConverter>();
        scoreModelsConverterStub.Convert(Arg.Any<ICollection<ScoreModel>?>()).Returns(eventScores);

        return new OutcomeConverter(scoreModelsConverterStub);
    }
}
