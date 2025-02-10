using FluentAssertions.Execution;
using OddsCollector.Functions.OddsApi.WebApi;
using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal class OriginalUpcomingEventConverter
{
    [Test]
    public void ToUpcomingEvents_WithOriginalEventData_ReturnsEventResult()
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
            new FunctionApp.BookmakerConverter(
                new FunctionApp.MarketConverter(
                    new FunctionApp.OutcomeConverter())));

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

        var converter = new FunctionApp.OriginalUpcomingEventConverter(bookmakerConverter);

        var upcomingEvent = converter.ToUpcomingEvents([]).ToList();

        upcomingEvent.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ToUpcomingEvents_WithNullEventData_ThrowsException()
    {
        var bookmakerConverter = Substitute.For<FunctionApp.IBookmakerConverter>();

        var converter = new FunctionApp.OriginalUpcomingEventConverter(bookmakerConverter);

        var action = () => converter.ToUpcomingEvents(null).ToList();

        action.Should().Throw<ArgumentNullException>().WithParameterName("originalEvents");
    }
}
