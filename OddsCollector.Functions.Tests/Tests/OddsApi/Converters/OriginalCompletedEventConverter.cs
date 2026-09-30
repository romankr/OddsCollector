using System.Globalization;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
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
            NullLogger<FunctionApp.OriginalCompletedEventConverter>.Instance,
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
    public void ToEventResults_WithUpcomingEvent_SkipsIt()
    {
        // Arrange
        var originalEventData = new Anonymous3
        {
            Away_team = "awayTeam",
            Commence_time = DateTime.UtcNow.AddDays(1),
            Completed = false,
            Home_team = "homeTeam",
            Id = Guid.NewGuid().ToString(),
            Scores = null
        };

        var converter = new FunctionApp.OriginalCompletedEventConverter(
            NullLogger<FunctionApp.OriginalCompletedEventConverter>.Instance,
            new FunctionApp.OutcomeConverter(
                new FunctionApp.ScoreModelsConverter(
                    new FunctionApp.ScoreModelConverter())));

        // Act
        var eventResults = converter.ToEventResults([originalEventData]).ToList();

        // Assert
        eventResults.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ToEventResults_WithLiveEvent_SkipsIt()
    {
        // Arrange
        var originalEventData = new Anonymous3
        {
            Away_team = "awayTeam",
            Commence_time = DateTime.UtcNow,
            Completed = false,
            Home_team = "homeTeam",
            Id = Guid.NewGuid().ToString(),
            Scores =
            [
                new ScoreModel { Name = "awayTeam", Score = "0" },
                new ScoreModel { Name = "homeTeam", Score = "1" }
            ]
        };

        var converter = new FunctionApp.OriginalCompletedEventConverter(
            NullLogger<FunctionApp.OriginalCompletedEventConverter>.Instance,
            new FunctionApp.OutcomeConverter(
                new FunctionApp.ScoreModelsConverter(
                    new FunctionApp.ScoreModelConverter())));

        // Act
        var eventResults = converter.ToEventResults([originalEventData]).ToList();

        // Assert
        eventResults.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ToEventResults_WithMixedEvents_ReturnsOnlyCompleted()
    {
        // Arrange
        var completedId = Guid.NewGuid().ToString();

        Anonymous3[] originalEvents =
        [
            new()
            {
                Away_team = "awayTeam",
                Commence_time = DateTime.UtcNow.AddDays(-1),
                Completed = true,
                Home_team = "homeTeam",
                Id = completedId,
                Scores =
                [
                    new ScoreModel { Name = "awayTeam", Score = "2" },
                    new ScoreModel { Name = "homeTeam", Score = "1" }
                ]
            },
            new()
            {
                Away_team = "awayTeam",
                Commence_time = DateTime.UtcNow.AddDays(1),
                Completed = false,
                Home_team = "homeTeam",
                Id = Guid.NewGuid().ToString(),
                Scores = null
            },
            new()
            {
                Away_team = "awayTeam",
                Commence_time = DateTime.UtcNow.AddDays(1),
                Completed = null,
                Home_team = "homeTeam",
                Id = Guid.NewGuid().ToString(),
                Scores = null
            }
        ];

        var converter = new FunctionApp.OriginalCompletedEventConverter(
            NullLogger<FunctionApp.OriginalCompletedEventConverter>.Instance,
            new FunctionApp.OutcomeConverter(
                new FunctionApp.ScoreModelsConverter(
                    new FunctionApp.ScoreModelConverter())));

        // Act
        var eventResults = converter.ToEventResults(originalEvents).ToList();

        // Assert
        eventResults.Should().ContainSingle().Which.Id.Should().Be(completedId);
        eventResults[0].Outcome.Should().Be("awayTeam");
    }

    [Test]
    public void ToEventResults_WithNoEventData_ReturnsNoEvents()
    {
        var converter = new FunctionApp.OriginalCompletedEventConverter(
            NullLogger<FunctionApp.OriginalCompletedEventConverter>.Instance,
            new FunctionApp.OutcomeConverter(
                new FunctionApp.ScoreModelsConverter(
                    new FunctionApp.ScoreModelConverter())));

        var eventResults = converter.ToEventResults([]).ToList();

        eventResults.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ToEventResults_WithNullEventData_ThrowsException()
    {
        var outcomeConverter = Substitute.For<FunctionApp.IOutcomeConverter>();

        var converter = new FunctionApp.OriginalCompletedEventConverter(
            NullLogger<FunctionApp.OriginalCompletedEventConverter>.Instance, outcomeConverter);

        var action = () => converter.ToEventResults(null);

        action.Should().Throw<ArgumentNullException>().WithParameterName("originalEvents");
    }

    [TestCase("abc", TestName = "ToEventResults_WithUnreadableScore_SkipsItAndReturnsTheRest")]
    [TestCase("", TestName = "ToEventResults_WithEmptyScore_SkipsItAndReturnsTheRest")]
    public void ToEventResults_WithBrokenScore_SkipsItAndReturnsTheRest(string score)
    {
        // Arrange
        var loggerMock = new FakeLogger<FunctionApp.OriginalCompletedEventConverter>();

        var converter = new FunctionApp.OriginalCompletedEventConverter(loggerMock, CreateOutcomeConverter());

        var broken = CreateCompletedEvent("broken");
        broken.Scores!.First().Score = score;

        var working = CreateCompletedEvent("working");

        // Act
        var eventResults = converter.ToEventResults([broken, working]).ToList();

        // Assert
        eventResults.Should().ContainSingle().Which.Id.Should().Be("working");

        loggerMock.Collector.Count.Should().Be(1);
        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Warning);
        loggerMock.LatestRecord.Message.Should().Be("Skipped event broken");
        loggerMock.LatestRecord.Exception.Should().BeAssignableTo<ArgumentException>();
    }

    [Test]
    public void ToEventResults_WithCompletedEventWithoutScores_SkipsIt()
    {
        // Arrange
        var loggerMock = new FakeLogger<FunctionApp.OriginalCompletedEventConverter>();

        var converter = new FunctionApp.OriginalCompletedEventConverter(loggerMock, CreateOutcomeConverter());

        var withoutScores = CreateCompletedEvent("withoutScores");
        withoutScores.Scores = null;

        var withOneScore = CreateCompletedEvent("withOneScore");
        withOneScore.Scores!.Remove(withOneScore.Scores.First());

        var working = CreateCompletedEvent("working");

        // Act
        var eventResults = converter.ToEventResults([withoutScores, withOneScore, working]).ToList();

        // Assert
        eventResults.Should().ContainSingle().Which.Id.Should().Be("working");
        loggerMock.Collector.Count.Should().Be(2);
    }

    private static FunctionApp.OutcomeConverter CreateOutcomeConverter()
    {
        return new FunctionApp.OutcomeConverter(
            new FunctionApp.ScoreModelsConverter(
                new FunctionApp.ScoreModelConverter()));
    }

    private static Anonymous3 CreateCompletedEvent(string id)
    {
        return new Anonymous3
        {
            Away_team = "awayTeam",
            Commence_time = DateTime.UtcNow.AddDays(-1),
            Completed = true,
            Home_team = "homeTeam",
            Id = id,
            Scores =
            [
                new ScoreModel { Name = "awayTeam", Score = "1" },
                new ScoreModel { Name = "homeTeam", Score = "2" }
            ]
        };
    }
}
