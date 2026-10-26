using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.UnitTests.Tests.OddsApi.Converters;

internal sealed class OutcomeConverterTests
{
    private const string FirstTeam = "firstTeam";
    private const string SecondTeam = "secondTeam";

    [TestCase(1, 1, OutcomeTypes.Draw, TestName = "GetOutcome_WithEqualScores_ReturnsDraw")]
    [TestCase(2, 1, FirstTeam, TestName = "GetOutcome_WithFirstTeamScoringMore_ReturnsFirstTeam")]
    [TestCase(1, 2, SecondTeam, TestName = "GetOutcome_WithSecondTeamScoringMore_ReturnsSecondTeam")]
    public void GetOutcome_WithScores_ReturnsWinnerOrDraw(int firstScore, int secondScore, string expectedOutcome)
    {
        // Arrange
        var scoreModelsConverterStub = Substitute.For<IScoreModelsConverter>();
        scoreModelsConverterStub.Convert(Arg.Any<ICollection<ScoreModel>?>()).Returns(
        [
            new EventScore { Name = FirstTeam, Score = firstScore },
            new EventScore { Name = SecondTeam, Score = secondScore }
        ]);

        var outcomeConverter = new OutcomeConverter(scoreModelsConverterStub);

        // Act
        var outcome = outcomeConverter.GetOutcome([]);

        // Assert
        outcome.Should().Be(expectedOutcome);
    }
}
