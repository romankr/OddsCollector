using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;
using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal sealed class OutcomeConverter
{
    private const string FirstTeam = "firstTeam";
    private const string SecondTeam = "secondTeam";

    [TestCase(1, 1, OutcomeTypes.Draw, TestName = "GetOutcome_WithEqualScores_ReturnsDraw")]
    [TestCase(2, 1, FirstTeam, TestName = "GetOutcome_WithFirstTeamScoringMore_ReturnsFirstTeam")]
    [TestCase(1, 2, SecondTeam, TestName = "GetOutcome_WithSecondTeamScoringMore_ReturnsSecondTeam")]
    public void GetOutcome_WithScores_ReturnsWinnerOrDraw(int firstScore, int secondScore, string expectedOutcome)
    {
        // Arrange
        var scoreModelsConverterStub = Substitute.For<FunctionApp.IScoreModelsConverter>();
        scoreModelsConverterStub.Convert(Arg.Any<ICollection<ScoreModel>?>()).Returns(
        [
            new FunctionApp.EventScore { Name = FirstTeam, Score = firstScore },
            new FunctionApp.EventScore { Name = SecondTeam, Score = secondScore }
        ]);

        var outcomeConverter = new FunctionApp.OutcomeConverter(scoreModelsConverterStub);

        // Act
        var outcome = outcomeConverter.GetOutcome([]);

        // Assert
        outcome.Should().Be(expectedOutcome);
    }
}
