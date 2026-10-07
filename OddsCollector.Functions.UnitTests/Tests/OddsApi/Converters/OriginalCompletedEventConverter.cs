using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;
using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal sealed class OriginalCompletedEventConverter
{
    private const string AwayTeam = "awayTeam";
    private const string HomeTeam = "homeTeam";

    private static readonly DateTime CommenceTime = new(2026, 9, 22, 18, 0, 0, DateTimeKind.Utc);

    [Test]
    public void ToEventResults_WithCompletedEvent_ReturnsEventResultWithWinnerAsOutcome()
    {
        // Arrange
        var converter = CreateConverter();

        var completedEvent = CreateCompletedEvent("id", awayScore: "1", homeScore: "2");

        // Act
        var eventResults = converter.ToEventResults([completedEvent]);

        // Assert
        eventResults.Should().ContainSingle().Which.Should().BeEquivalentTo(new EventResult
        {
            Id = "id",
            CommenceTime = CommenceTime,
            Outcome = HomeTeam
        });
    }

    [TestCase(false, TestName = "ToEventResults_WithEventNotCompleted_SkipsIt")]
    [TestCase(null, TestName = "ToEventResults_WithEventOfUnknownState_SkipsIt")]
    public void ToEventResults_WithEventNotKnownToBeCompleted_SkipsIt(bool? completed)
    {
        // Arrange
        var converter = CreateConverter();

        var liveEvent = CreateCompletedEvent("live", awayScore: "0", homeScore: "1");
        liveEvent.Completed = completed;

        // Act
        var eventResults = converter.ToEventResults([liveEvent]);

        // Assert
        eventResults.Should().BeEmpty();
    }

    [Test]
    public void ToEventResults_WithNoEvents_ReturnsNoEventResults()
    {
        // Arrange
        var converter = CreateConverter();

        // Act
        var eventResults = converter.ToEventResults([]);

        // Assert
        eventResults.Should().BeEmpty();
    }

    [Test]
    public void ToEventResults_WithNullEvents_ThrowsArgumentNullException()
    {
        // Arrange
        var converter = new FunctionApp.OriginalCompletedEventConverter(
            NullLogger<FunctionApp.OriginalCompletedEventConverter>.Instance,
            Substitute.For<FunctionApp.IOutcomeConverter>());

        // Act
        var action = () => converter.ToEventResults(null);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("originalEvents");
    }

    [TestCase("abc", TestName = "ToEventResults_WithUnreadableScore_SkipsItAndReturnsTheRest")]
    [TestCase("", TestName = "ToEventResults_WithEmptyScore_SkipsItAndReturnsTheRest")]
    public void ToEventResults_WithBrokenScore_SkipsItAndReturnsTheRest(string score)
    {
        // Arrange
        var loggerMock = new FakeLogger<FunctionApp.OriginalCompletedEventConverter>();

        var converter = CreateConverter(loggerMock);

        var broken = CreateCompletedEvent("broken", awayScore: score, homeScore: "2");
        var working = CreateCompletedEvent("working", awayScore: "1", homeScore: "2");

        // Act
        var eventResults = converter.ToEventResults([broken, working]).ToList();

        // Assert
        using var scope = new AssertionScope();

        eventResults.Should().ContainSingle().Which.Id.Should().Be("working");

        var record = loggerMock.Collector.GetSnapshot().Should().ContainSingle().Subject;
        record.Level.Should().Be(LogLevel.Warning);
        record.Message.Should().Be("Skipped event broken");
        record.Exception.Should().BeAssignableTo<ArgumentException>();
    }

    [Test]
    public void ToEventResults_WithCompletedEventWithoutScores_SkipsIt()
    {
        // Arrange
        var loggerMock = new FakeLogger<FunctionApp.OriginalCompletedEventConverter>();

        var converter = CreateConverter(loggerMock);

        var withoutScores = CreateCompletedEvent("withoutScores", awayScore: "1", homeScore: "2");
        withoutScores.Scores = null;

        var withOneScore = CreateCompletedEvent("withOneScore", awayScore: "1", homeScore: "2");
        withOneScore.Scores = [new ScoreModel { Name = HomeTeam, Score = "2" }];

        var working = CreateCompletedEvent("working", awayScore: "1", homeScore: "2");

        // Act
        var eventResults = converter.ToEventResults([withoutScores, withOneScore, working]).ToList();

        // Assert
        using var scope = new AssertionScope();

        eventResults.Should().ContainSingle().Which.Id.Should().Be("working");
        loggerMock.Collector.GetSnapshot().Select(r => r.Message).Should().Equal(
            "Skipped event withoutScores",
            "Skipped event withOneScore");
    }

    private static FunctionApp.OriginalCompletedEventConverter CreateConverter(
        ILogger<FunctionApp.OriginalCompletedEventConverter>? logger = null)
    {
        return new FunctionApp.OriginalCompletedEventConverter(
            logger ?? NullLogger<FunctionApp.OriginalCompletedEventConverter>.Instance,
            new FunctionApp.OutcomeConverter(
                new FunctionApp.ScoreModelsConverter(
                    new FunctionApp.ScoreModelConverter())));
    }

    private static Anonymous3 CreateCompletedEvent(string id, string awayScore, string homeScore)
    {
        return new Anonymous3
        {
            Away_team = AwayTeam,
            Commence_time = CommenceTime,
            Completed = true,
            Home_team = HomeTeam,
            Id = id,
            Scores =
            [
                new ScoreModel { Name = AwayTeam, Score = awayScore },
                new ScoreModel { Name = HomeTeam, Score = homeScore }
            ]
        };
    }
}
