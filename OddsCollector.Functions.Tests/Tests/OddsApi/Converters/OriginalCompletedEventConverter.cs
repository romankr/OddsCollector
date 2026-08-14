using System.Globalization;
using FluentAssertions.Execution;
using OddsCollector.Functions.OddsApi.WebApi;
using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal sealed class OriginalCompletedEventConverter
{
    [Test]
    public void ToEventResult_WithOriginalEventData_ReturnsEventResult()
    {
        // Arrange
        var expectedCommenceTime = DateTime.UtcNow;
        const string expectedOutcome = "homeTeam";
        var expectedId = Guid.NewGuid().ToString();

        var originalEventData = new Anonymous3
        {
            Away_team = "awayTeam",
            Commence_time = expectedCommenceTime,
            Completed = true,
            Home_team = expectedOutcome,
            Id = expectedId,
            Last_update = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
            Scores =
            [
                new ScoreModel { Name = "awayTeam", Score = "1" },
                new ScoreModel { Name = expectedOutcome, Score = "2" }
            ]
        };

        var converter = new FunctionApp.OriginalCompletedEventConverter(
            new FunctionApp.OutcomeConverter(
                new FunctionApp.ScoreModelsConverter(
                    new FunctionApp.ScoreModelConverter())));

        // Act
        var eventResults = converter.ToEventResults([originalEventData]).ToList();

        // Assert
        eventResults.Should().NotBeNull().And.HaveCount(1);

        using var scope = new AssertionScope();

        eventResults[0].CommenceTime.Should().Be(expectedCommenceTime);
        eventResults[0].Outcome.Should().Be(expectedOutcome);
        eventResults[0].Id.Should().Be(expectedId);
    }

    [Test]
    public void ToEventResult_WithNoEventData_ReturnsNoEvents()
    {
        var converter = new FunctionApp.OriginalCompletedEventConverter(
            new FunctionApp.OutcomeConverter(
                new FunctionApp.ScoreModelsConverter(
                    new FunctionApp.ScoreModelConverter())));

        var eventResults = converter.ToEventResults([]).ToList();

        eventResults.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ToEventResult_WithNullEventData_ThrowsException()
    {
        var outcomeConverter = Substitute.For<FunctionApp.IOutcomeConverter>();

        var eventConverter = new FunctionApp.OriginalCompletedEventConverter(outcomeConverter);

        var action = () => eventConverter.ToEventResults(null).ToList();

        action.Should().Throw<ArgumentNullException>().WithParameterName("originalEvents");
    }
}
