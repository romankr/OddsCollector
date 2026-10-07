using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;
using OddsCollector.Functions.Tests.Infrastructure.Time;
using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal sealed class OriginalUpcomingEventConverter
{
    private const string AwayTeam = "awayTeam";
    private const string HomeTeam = "homeTeam";
    private const string Bookmaker = "bookmaker";
    private const double HomePrice = 1.5;
    private const double AwayPrice = 4.0;
    private const double DrawPrice = 3.5;

    private static readonly DateTime CommenceTime = FixedTimeProvider.Now.UtcDateTime.AddHours(1);

    [Test]
    public void ToUpcomingEvents_WithUpcomingEvent_ReturnsItWithItsOdds()
    {
        // Arrange
        var converter = CreateConverter();

        // Act
        var upcomingEvents = converter.ToUpcomingEvents([CreateEvent("id")]);

        // Assert
        upcomingEvents.Should().ContainSingle().Which.Should().BeEquivalentTo(new UpcomingEvent
        {
            Id = "id",
            AwayTeam = AwayTeam,
            HomeTeam = HomeTeam,
            CommenceTime = CommenceTime,
            Odds = [new Odd { Bookmaker = Bookmaker, Away = AwayPrice, Draw = DrawPrice, Home = HomePrice }]
        });
    }

    [Test]
    public void ToUpcomingEvents_WithNoEvents_ReturnsNoUpcomingEvents()
    {
        // Arrange
        var converter = CreateConverter();

        // Act
        var upcomingEvents = converter.ToUpcomingEvents([]);

        // Assert
        upcomingEvents.Should().BeEmpty();
    }

    [Test]
    public void ToUpcomingEvents_WithNullEvents_ThrowsArgumentNullException()
    {
        // Arrange
        var converter = new FunctionApp.OriginalUpcomingEventConverter(
            NullLogger<FunctionApp.OriginalUpcomingEventConverter>.Instance,
            Substitute.For<FunctionApp.IBookmakerConverter>(), FixedTimeProvider.AtNow);

        // Act
        var action = () => converter.ToUpcomingEvents(null);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("events");
    }

    [Test]
    public void ToUpcomingEvents_WithMalformedEvent_SkipsItAndReturnsTheRest()
    {
        // Arrange
        var loggerMock = new FakeLogger<FunctionApp.OriginalUpcomingEventConverter>();

        var converter = CreateConverter(loggerMock);

        var malformed = CreateEvent("malformed");
        malformed.Home_team = null;

        var working = CreateEvent("working");

        // Act
        var upcomingEvents = converter.ToUpcomingEvents([malformed, working]).ToList();

        // Assert
        using var scope = new AssertionScope();

        upcomingEvents.Should().ContainSingle().Which.Id.Should().Be("working");

        var record = loggerMock.Collector.GetSnapshot().Should().ContainSingle().Subject;
        record.Level.Should().Be(LogLevel.Warning);
        record.Message.Should().Be("Skipped event malformed");
        record.Exception.Should().BeAssignableTo<ArgumentException>();
    }

    [Test]
    public void ToUpcomingEvents_WithoutUsableOdds_SkipsEvent()
    {
        // Arrange
        var loggerMock = new FakeLogger<FunctionApp.OriginalUpcomingEventConverter>();

        var converter = CreateConverter(loggerMock);

        var withoutBookmakers = CreateEvent("withoutBookmakers");
        withoutBookmakers.Bookmakers = [];

        var withBrokenBookmaker = CreateEvent("withBrokenBookmaker");
        withBrokenBookmaker.Bookmakers = [new Bookmakers { Key = Bookmaker, Markets = [] }];

        // Act
        var upcomingEvents = converter.ToUpcomingEvents([withoutBookmakers, withBrokenBookmaker]).ToList();

        // Assert: an event with nothing to predict from would only fail further on.
        using var scope = new AssertionScope();

        upcomingEvents.Should().BeEmpty();
        loggerMock.Collector.GetSnapshot().Select(r => r.Message).Should().Equal(
            "Skipped event withoutBookmakers: no usable odds",
            "Skipped event withBrokenBookmaker: no usable odds");
    }

    [Test]
    public void ToUpcomingEvents_WithOneBrokenBookmaker_KeepsEventWithTheOthers()
    {
        // Arrange
        var converter = CreateConverter();

        var upcomingEvent = CreateEvent("event");
        upcomingEvent.Bookmakers!.Add(new Bookmakers { Key = "broken", Markets = [] });

        // Act
        var upcomingEvents = converter.ToUpcomingEvents([upcomingEvent]);

        // Assert
        upcomingEvents.Should().ContainSingle().Which.Odds.Should().ContainSingle()
            .Which.Bookmaker.Should().Be(Bookmaker);
    }

    [TestCase(0, TestName = "ToUpcomingEvents_WithEventStartingNow_SkipsIt")]
    [TestCase(-90, TestName = "ToUpcomingEvents_WithEventInPlay_SkipsIt")]
    public void ToUpcomingEvents_WithStartedEvent_SkipsIt(int minutesFromNow)
    {
        // Arrange
        var loggerMock = new FakeLogger<FunctionApp.OriginalUpcomingEventConverter>();

        var converter = CreateConverter(loggerMock);

        var started = CreateEvent("started");
        started.Commence_time = FixedTimeProvider.Now.UtcDateTime.AddMinutes(minutesFromNow);

        var upcoming = CreateEvent("upcoming");

        // Act
        var upcomingEvents = converter.ToUpcomingEvents([started, upcoming]).ToList();

        // Assert: odds of a game in play already reflect the score.
        using var scope = new AssertionScope();

        upcomingEvents.Should().ContainSingle().Which.Id.Should().Be("upcoming");
        loggerMock.Collector.GetSnapshot().Should().ContainSingle().Which.Should().BeEquivalentTo(
            new { Level = LogLevel.Debug, Message = "Skipped event started: already started" });
    }

    private static FunctionApp.OriginalUpcomingEventConverter CreateConverter(
        ILogger<FunctionApp.OriginalUpcomingEventConverter>? logger = null)
    {
        return new FunctionApp.OriginalUpcomingEventConverter(
            logger ?? NullLogger<FunctionApp.OriginalUpcomingEventConverter>.Instance,
            new FunctionApp.BookmakerConverter(NullLogger<FunctionApp.BookmakerConverter>.Instance,
                new FunctionApp.MarketConverter(new FunctionApp.OddConverter())),
            FixedTimeProvider.AtNow);
    }

    private static Anonymous2 CreateEvent(string id)
    {
        return new Anonymous2
        {
            Away_team = AwayTeam,
            Commence_time = CommenceTime,
            Home_team = HomeTeam,
            Id = id,
            Bookmakers =
            [
                new Bookmakers
                {
                    Key = Bookmaker,
                    Markets =
                    [
                        new Markets2
                        {
                            Key = Markets2Key.H2h,
                            Outcomes =
                            [
                                new Outcome { Name = HomeTeam, Price = HomePrice },
                                new Outcome { Name = AwayTeam, Price = AwayPrice },
                                new Outcome { Name = OutcomeTypes.Draw, Price = DrawPrice }
                            ]
                        }
                    ]
                }
            ]
        };
    }
}
