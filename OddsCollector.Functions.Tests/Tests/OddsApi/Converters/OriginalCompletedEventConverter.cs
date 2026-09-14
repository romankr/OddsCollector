using System.Globalization;
using FluentAssertions.Execution;
using OddsCollector.Functions.OddsApi.WebApi;
using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal sealed class OriginalCompletedEventConverter
{
    [Test]
    public void ToEventResults_WithOriginalEventData_ReturnsEventResult()
    {
        // Arrange
        var expectedCommenceTime = DateTime.UtcNow;
        const string expectedWinner = "homeTeam";
        var expectedId = Guid.NewGuid().ToString();

        var originalEventData = new Anonymous3
        {
            Away_team = "awayTeam",
            Commence_time = expectedCommenceTime,
            Completed = true,
            Home_team = expectedWinner,
            Id = expectedId,
            Last_update = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
            Scores =
            [
                new ScoreModel { Name = "awayTeam", Score = "1" },
                new ScoreModel { Name = expectedWinner, Score = "2" }
            ]
        };

        var converter = new FunctionApp.OriginalCompletedEventConverter(
            new FunctionApp.WinnerConverter(
                new FunctionApp.ScoreModelsConverter(
                    new FunctionApp.ScoreModelConverter())));

        // Act
        var eventResults = converter.ToEventResults([originalEventData]).ToList();

        // Assert
        eventResults.Should().NotBeNull().And.HaveCount(1);

        using var scope = new AssertionScope();

        eventResults[0].CommenceTime.Should().Be(expectedCommenceTime);
        eventResults[0].Winner.Should().Be(expectedWinner);
        eventResults[0].Id.Should().Be(expectedId);
    }

    [Test]
    public void ToEventResults_WithNoEventData_ReturnsNoEvents()
    {
        var converter = new FunctionApp.OriginalCompletedEventConverter(
            new FunctionApp.WinnerConverter(
                new FunctionApp.ScoreModelsConverter(
                    new FunctionApp.ScoreModelConverter())));

        var eventResults = converter.ToEventResults([]).ToList();

        eventResults.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ToEventResults_WithNullEventData_ThrowsException()
    {
        var winnerConverter = Substitute.For<FunctionApp.IWinnerConverter>();

        var converter = new FunctionApp.OriginalCompletedEventConverter(winnerConverter);

        var action = () => converter.ToEventResults(null).ToList();

        action.Should().Throw<ArgumentNullException>().WithParameterName("originalEvents");
    }
}
