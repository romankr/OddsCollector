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
        var conversion = converter.GetOutcome([], HomeTeam, AwayTeam);

        // Assert
        conversion.Value.Should().Be(expectedOutcome);
    }

    [Test]
    public void GetOutcome_WithAwayTeamListedFirst_MatchesScoresByTeamName()
    {
        // Arrange
        var converter = CreateConverter(
            new EventScore { Name = AwayTeam, Score = 0 },
            new EventScore { Name = HomeTeam, Score = 3 });

        // Act
        var conversion = converter.GetOutcome([], HomeTeam, AwayTeam);

        // Assert
        conversion.Value.Should().Be(OutcomeType.HomeTeam);
    }

    [TestCase(HomeTeam, "otherTeam", AwayTeam,
        TestName = "GetOutcome_WithoutAwayTeamScore_ReturnsRejectionNamingIt")]
    [TestCase("otherTeam", AwayTeam, HomeTeam,
        TestName = "GetOutcome_WithoutHomeTeamScore_ReturnsRejectionNamingIt")]
    public void GetOutcome_WithoutTeamScore_ReturnsRejectionNamingIt(string firstScoredTeam,
        string secondScoredTeam, string missingTeam)
    {
        // Arrange
        var converter = CreateConverter(
            new EventScore { Name = firstScoredTeam, Score = 1 },
            new EventScore { Name = secondScoredTeam, Score = 0 });

        // Act
        var conversion = converter.GetOutcome([], HomeTeam, AwayTeam);

        // Assert
        conversion.Value.Should().Be(new Rejection($"no score of {missingTeam}"));
    }

    [Test]
    public void GetOutcome_WithRejectedScores_ReturnsTheirRejection()
    {
        // Arrange
        var expectedRejection = new Rejection("reason");

        Conversion<EventScore[]> rejected = expectedRejection;

        var scoreModelsConverterStub = Substitute.For<IScoreModelsConverter>();
        scoreModelsConverterStub.Convert(Arg.Any<ICollection<ScoreModel>?>()).Returns(rejected);

        var converter = new OutcomeConverter(scoreModelsConverterStub);

        // Act
        var conversion = converter.GetOutcome(null, HomeTeam, AwayTeam);

        // Assert
        conversion.Value.Should().BeSameAs(expectedRejection);
    }

    [TestCase(null, TestName = "GetOutcome_WithNullAwayTeam_ThrowsArgumentException")]
    [TestCase(" ", TestName = "GetOutcome_WithWhitespaceAwayTeam_ThrowsArgumentException")]
    public void GetOutcome_WithBlankAwayTeam_ThrowsArgumentException(string? awayTeam)
    {
        // Arrange
        var converter = CreateConverter();

        // Act
        var action = () => converter.GetOutcome([], HomeTeam, awayTeam!);

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
        var action = () => converter.GetOutcome([], homeTeam!, AwayTeam);

        // Assert
        action.Should().Throw<ArgumentException>().WithParameterName(nameof(homeTeam));
    }

    private static OutcomeConverter CreateConverter(params EventScore[] eventScores)
    {
        Conversion<EventScore[]> converted = eventScores;

        var scoreModelsConverterStub = Substitute.For<IScoreModelsConverter>();
        scoreModelsConverterStub.Convert(Arg.Any<ICollection<ScoreModel>?>()).Returns(converted);

        return new OutcomeConverter(scoreModelsConverterStub);
    }
}
