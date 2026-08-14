using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;
using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal sealed class OutcomeConverter
{
    [Test]
    public void GetOutcome_WithDraw_ReturnsDraw()
    {
        // Arrange
        var modelsConverter = Substitute.For<FunctionApp.IScoreModelsConverter>();

        ICollection<FunctionApp.EventScore> eventScores =
        [
            new() { Name = "firstTeam", Score = 1 },
            new() { Name = "secondTeam", Score = 1 }
        ];

        modelsConverter.Convert(Arg.Any<ICollection<ScoreModel>?>()).Returns(eventScores);

        var outcomeConverter = new FunctionApp.OutcomeConverter(modelsConverter);

        // Act
        var outcome = outcomeConverter.GetOutcome([]);

        // Assert
        outcome.Should().NotBeNull().And.Be(OutcomeTypes.Draw);
    }

    [Test]
    public void GetOutcome_WithWinningOutcomeAtFirstElement_ReturnsWinner()
    {
        // Arrange
        var modelsConverter = Substitute.For<FunctionApp.IScoreModelsConverter>();

        const string expectedOutcome = "firstTeam";

        ICollection<FunctionApp.EventScore> eventScores =
        [
            new() { Name = expectedOutcome, Score = 2 },
            new() { Name = "secondTeam", Score = 1 }
        ];

        modelsConverter.Convert(Arg.Any<ICollection<ScoreModel>?>()).Returns(eventScores);

        var outcomeConverter = new FunctionApp.OutcomeConverter(modelsConverter);

        // Act
        var outcome = outcomeConverter.GetOutcome([]);

        // Assert
        outcome.Should().NotBeNull().And.Be(expectedOutcome);
    }

    [Test]
    public void GetOutcome_WithWinningOutcomeAtSecondElement_ReturnsOutcome()
    {
        // Arrange
        var modelsConverter = Substitute.For<FunctionApp.IScoreModelsConverter>();

        const string expectedOutcome = "secondTeam";

        ICollection<FunctionApp.EventScore> eventScores =
        [
            new() { Name = "firstTeam", Score = 1 },
            new() { Name = expectedOutcome, Score = 2 }
        ];

        modelsConverter.Convert(Arg.Any<ICollection<ScoreModel>?>()).Returns(eventScores);

        var outcomeConverter = new FunctionApp.OutcomeConverter(modelsConverter);

        // Act
        var outcome = outcomeConverter.GetOutcome([]);

        // Assert
        outcome.Should().NotBeNull().And.Be(expectedOutcome);
    }

    [Test]
    public void GetOutcome_WithNullScore_ThrowsException()
    {
        var converter = new FunctionApp.ScoreModelConverter();

        var action = () => converter.ToEventScore(new ScoreModel());

        action.Should().Throw<ArgumentNullException>().WithParameterName("scoreModel.Name");
    }

    [Test]
    public void GetOutcome_WithNonIntegerScore_ThrowsException()
    {
        var converter = new FunctionApp.ScoreModelConverter();

        var action = () => converter.ToEventScore(new ScoreModel { Score = "test", Name = "name" });

        action.Should().Throw<ArgumentException>().WithParameterName("scoreModel");
    }
}
