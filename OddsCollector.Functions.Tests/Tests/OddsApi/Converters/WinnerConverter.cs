using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;
using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal sealed class WinnerConverter
{
    [Test]
    public void GetWinner_WithDraw_ReturnsDraw()
    {
        // Arrange
        var modelsConverter = Substitute.For<FunctionApp.IScoreModelsConverter>();

        ICollection<FunctionApp.EventScore> eventScores =
        [
            new() { Name = "firstTeam", Score = 1 },
            new() { Name = "secondTeam", Score = 1 }
        ];

        modelsConverter.Convert(Arg.Any<ICollection<ScoreModel>?>()).Returns(eventScores);

        var winnerConverter = new FunctionApp.WinnerConverter(modelsConverter);

        // Act
        var winner = winnerConverter.GetWinner([]);

        // Assert
        winner.Should().NotBeNull().And.Be(OutcomeTypes.Draw);
    }

    [Test]
    public void GetWinner_WithWinnerAtFirstElement_ReturnsWinner()
    {
        // Arrange
        var modelsConverter = Substitute.For<FunctionApp.IScoreModelsConverter>();

        const string expectedWinner = "firstTeam";

        ICollection<FunctionApp.EventScore> eventScores =
        [
            new() { Name = expectedWinner, Score = 2 },
            new() { Name = "secondTeam", Score = 1 }
        ];

        modelsConverter.Convert(Arg.Any<ICollection<ScoreModel>?>()).Returns(eventScores);

        var winnerConverter = new FunctionApp.WinnerConverter(modelsConverter);

        // Act
        var winner = winnerConverter.GetWinner([]);

        // Assert
        winner.Should().NotBeNull().And.Be(expectedWinner);
    }

    [Test]
    public void GetWinner_WithWinnerAtSecondElement_ReturnsWinner()
    {
        // Arrange
        var modelsConverter = Substitute.For<FunctionApp.IScoreModelsConverter>();

        const string expectedWinner = "secondTeam";

        ICollection<FunctionApp.EventScore> eventScores =
        [
            new() { Name = "firstTeam", Score = 1 },
            new() { Name = expectedWinner, Score = 2 }
        ];

        modelsConverter.Convert(Arg.Any<ICollection<ScoreModel>?>()).Returns(eventScores);

        var winnerConverter = new FunctionApp.WinnerConverter(modelsConverter);

        // Act
        var winner = winnerConverter.GetWinner([]);

        // Assert
        winner.Should().NotBeNull().And.Be(expectedWinner);
    }

    [Test]
    public void ToEGetWinner_WithNullScore_ThrowsException()
    {
        var converter = new FunctionApp.ScoreModelConverter();

        var action = () => converter.ToEventScore(new ScoreModel());

        action.Should().Throw<ArgumentNullException>().WithParameterName("scoreModel.Name");
    }

    [Test]
    public void ToEGetWinner_WithNonIntegerScore_ThrowsException()
    {
        var converter = new FunctionApp.ScoreModelConverter();

        var action = () => converter.ToEventScore(new ScoreModel { Score = "test", Name = "name" });

        action.Should().Throw<ArgumentException>().WithParameterName("Score");
    }
}
