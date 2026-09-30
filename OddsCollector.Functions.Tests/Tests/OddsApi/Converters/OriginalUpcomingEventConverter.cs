using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using OddsCollector.Functions.OddsApi.WebApi;
using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal class OriginalUpcomingEventConverter
{
    [Test]
    public void ToUpcomingEvents_WithOriginalEventData_ReturnsUpcomingEvent()
    {
        // Arrange
        var expectedCommenceTime = DateTime.UtcNow;
        var expectedHomeTeam = "homeTeam";
        var expectedAwayTeam = "awayTeam";
        var expectedId = Guid.NewGuid().ToString();
        var expectedBookmaker = "bookmaker";
        var expectedHomeScore = 1.1;
        var expectedAwayScore = 1.2;
        var expectedDrawScore = 1.3;

        var originalEvent = new Anonymous2
        {
            Away_team = expectedAwayTeam,
            Commence_time = expectedCommenceTime,
            Home_team = expectedHomeTeam,
            Id = expectedId,
            Bookmakers =
            [
                new Bookmakers
                {
                    Key = expectedBookmaker,
                    Markets =
                    [
                        new Markets2
                        {
                            Key = Markets2Key.H2h,
                            Outcomes =
                            [
                                new Outcome { Name = expectedHomeTeam, Price = expectedHomeScore },
                                new Outcome { Name = expectedAwayTeam, Price = expectedAwayScore },
                                new Outcome { Name = "Draw", Price = expectedDrawScore }
                            ]
                        }
                    ]
                }
            ]
        };

        var converter = new FunctionApp.OriginalUpcomingEventConverter(
            NullLogger<FunctionApp.OriginalUpcomingEventConverter>.Instance,
            new FunctionApp.BookmakerConverter(NullLogger<FunctionApp.BookmakerConverter>.Instance,
                new FunctionApp.MarketConverter(
                    new FunctionApp.OddConverter())));

        // Act
        var upcomingEvent = converter.ToUpcomingEvents([originalEvent]).ToList();

        // Assert
        upcomingEvent.Should().NotBeNull().And.HaveCount(1);

        using var scope = new AssertionScope();

        upcomingEvent[0].CommenceTime.Should().Be(expectedCommenceTime);
        upcomingEvent[0].Id.Should().Be(expectedId);
        upcomingEvent[0].HomeTeam.Should().Be(expectedHomeTeam);
        upcomingEvent[0].AwayTeam.Should().Be(expectedAwayTeam);
        upcomingEvent[0].Odds.Should().NotBeNull().And.HaveCount(1);

        var odd = upcomingEvent[0].Odds.ElementAt(0);

        odd.Away.Should().BeApproximately(expectedAwayScore, 0.01);
        odd.Draw.Should().BeApproximately(expectedDrawScore, 0.01);
        odd.Home.Should().BeApproximately(expectedHomeScore, 0.01);
        odd.Bookmaker.Should().Be(expectedBookmaker);
    }

    [Test]
    public void ToUpcomingEvents_WithNoEventData_ReturnsNoEvents()
    {
        var bookmakerConverter = Substitute.For<FunctionApp.IBookmakerConverter>();

        var converter = new FunctionApp.OriginalUpcomingEventConverter(
            NullLogger<FunctionApp.OriginalUpcomingEventConverter>.Instance, bookmakerConverter);

        var upcomingEvent = converter.ToUpcomingEvents([]).ToList();

        upcomingEvent.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ToUpcomingEvents_WithNullEventData_ThrowsException()
    {
        var bookmakerConverter = Substitute.For<FunctionApp.IBookmakerConverter>();

        var converter = new FunctionApp.OriginalUpcomingEventConverter(
            NullLogger<FunctionApp.OriginalUpcomingEventConverter>.Instance, bookmakerConverter);

        var action = () => converter.ToUpcomingEvents(null);

        action.Should().Throw<ArgumentNullException>().WithParameterName("events");
    }

    [Test]
    public void ToUpcomingEvents_WithMalformedEvent_SkipsItAndReturnsTheRest()
    {
        // Arrange
        var loggerMock = new FakeLogger<FunctionApp.OriginalUpcomingEventConverter>();

        var converter = new FunctionApp.OriginalUpcomingEventConverter(loggerMock, CreateBookmakerConverter());

        var malformed = CreateEvent("malformed");
        malformed.Home_team = null;

        var working = CreateEvent("working");

        // Act
        var upcomingEvents = converter.ToUpcomingEvents([malformed, working]).ToList();

        // Assert
        upcomingEvents.Should().ContainSingle().Which.Id.Should().Be("working");

        loggerMock.Collector.Count.Should().Be(1);
        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Warning);
        loggerMock.LatestRecord.Message.Should().Be("Skipped event malformed");
        loggerMock.LatestRecord.Exception.Should().BeAssignableTo<ArgumentException>();
    }

    [Test]
    public void ToUpcomingEvents_WithoutUsableOdds_SkipsEvent()
    {
        // Arrange
        var loggerMock = new FakeLogger<FunctionApp.OriginalUpcomingEventConverter>();

        var converter = new FunctionApp.OriginalUpcomingEventConverter(loggerMock, CreateBookmakerConverter());

        var withoutBookmakers = CreateEvent("withoutBookmakers");
        withoutBookmakers.Bookmakers = [];

        var withBrokenBookmaker = CreateEvent("withBrokenBookmaker");
        withBrokenBookmaker.Bookmakers!.First().Markets = [];

        // Act
        var upcomingEvents = converter.ToUpcomingEvents([withoutBookmakers, withBrokenBookmaker]).ToList();

        // Assert: an event with nothing to predict from would only fail further on.
        upcomingEvents.Should().BeEmpty();

        loggerMock.Collector.GetSnapshot().Select(r => r.Message).Should().Equal(
            "Skipped event withoutBookmakers: no usable odds",
            "Skipped event withBrokenBookmaker: no usable odds");
    }

    [Test]
    public void ToUpcomingEvents_WithOneBrokenBookmaker_KeepsEventWithTheOthers()
    {
        // Arrange
        var converter = new FunctionApp.OriginalUpcomingEventConverter(
            NullLogger<FunctionApp.OriginalUpcomingEventConverter>.Instance, CreateBookmakerConverter());

        var originalEvent = CreateEvent("event");
        originalEvent.Bookmakers!.Add(new Bookmakers { Key = "broken", Markets = [] });

        // Act
        var upcomingEvents = converter.ToUpcomingEvents([originalEvent]).ToList();

        // Assert
        upcomingEvents.Should().ContainSingle().Which.Odds.Should().ContainSingle()
            .Which.Bookmaker.Should().Be("bookmaker");
    }

    private static FunctionApp.BookmakerConverter CreateBookmakerConverter()
    {
        return new FunctionApp.BookmakerConverter(NullLogger<FunctionApp.BookmakerConverter>.Instance,
            new FunctionApp.MarketConverter(new FunctionApp.OddConverter()));
    }

    private static Anonymous2 CreateEvent(string id)
    {
        return new Anonymous2
        {
            Away_team = "awayTeam",
            Commence_time = DateTime.UtcNow,
            Home_team = "homeTeam",
            Id = id,
            Bookmakers =
            [
                new Bookmakers
                {
                    Key = "bookmaker",
                    Markets =
                    [
                        new Markets2
                        {
                            Key = Markets2Key.H2h,
                            Outcomes =
                            [
                                new Outcome { Name = "homeTeam", Price = 1.5 },
                                new Outcome { Name = "awayTeam", Price = 4.0 },
                                new Outcome { Name = "Draw", Price = 3.5 }
                            ]
                        }
                    ]
                }
            ]
        };
    }
}
