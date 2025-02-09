using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;
using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal sealed class WinnerConverter
{
    [Test]
    public void GetWinner_WithDraw_ReturnsDraw()
    {
        // Arrange
        var modelsConverter = Substitute.For<IScoreModelsConverter>();

        ICollection<EventScore> eventScores =
        [
            new EventScore()
            {
                Name = "firstTeam",
                Score = 1
            },
            new EventScore()
            {
                Name = "secondTeam",
                Score = 1
            }
        ];

        modelsConverter.Convert(Arg.Any<ICollection<ScoreModel>?>()).Returns(eventScores);

        var winnerConverter = new FunctionApp.WinnerConverter(modelsConverter);

        // Act
        var winner = winnerConverter.GetWinner(null);

        // Assert
        winner.Should().NotBeNull().And.Be(OutcomeTypes.Draw);
    }

    [Test]
    public void GetWinner_WithWinnerAtFirstElement_ReturnsWinner()
    {
        // Arrange
        var modelsConverter = Substitute.For<IScoreModelsConverter>();

        const string expectedWinner = "firstTeam";

        ICollection<EventScore> eventScores =
        [
            new EventScore()
            {
                Name = expectedWinner,
                Score = 2
            },
            new EventScore()
            {
                Name = "secondTeam",
                Score = 1
            }
        ];

        modelsConverter.Convert(Arg.Any<ICollection<ScoreModel>?>()).Returns(eventScores);

        var winnerConverter = new FunctionApp.WinnerConverter(modelsConverter);

        // Act
        var winner = winnerConverter.GetWinner(null);

        // Assert
        winner.Should().NotBeNull().And.Be(expectedWinner);
    }

    [Test]
    public void GetWinner_WithWinnerAtSecondElement_ReturnsWinner()
    {
        // Arrange
        var modelsConverter = Substitute.For<IScoreModelsConverter>();

        const string expectedWinner = "secondTeam";

        ICollection<EventScore> eventScores =
        [
            new EventScore()
            {
                Name = "firstTeam",
                Score = 1
            },
            new EventScore()
            {
                Name = expectedWinner,
                Score = 2
            }
        ];

        modelsConverter.Convert(Arg.Any<ICollection<ScoreModel>?>()).Returns(eventScores);

        var winnerConverter = new FunctionApp.WinnerConverter(modelsConverter);

        // Act
        var winner = winnerConverter.GetWinner(null);

        // Assert
        winner.Should().NotBeNull().And.Be(expectedWinner);
    }
}
