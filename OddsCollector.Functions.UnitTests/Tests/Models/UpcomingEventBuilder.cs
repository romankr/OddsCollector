using FunctionApp = OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Tests.Models;

internal sealed class UpcomingEventBuilder
{
    [TestCase("", TestName = "SetAwayTeam_WithEmptyString_ThrowsException")]
    [TestCase(null, TestName = "SetAwayTeam_WithNullString_ThrowsException")]
    [TestCase(" ", TestName = "SetAwayTeam_WithWhitespaceString_ThrowsException")]
    public void SetAwayTeam_WithNullOrEmptyString_ThrowsException(string? awayTeam)
    {
        var builder = new FunctionApp.UpcomingEventBuilder();

        var action = () => builder.SetAwayTeam(awayTeam);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(awayTeam));
    }

    [TestCase("", TestName = "SetHomeTeam_WithEmptyString_ThrowsException")]
    [TestCase(null, TestName = "SetHomeTeam_WithNullString_ThrowsException")]
    [TestCase(" ", TestName = "SetHomeTeam_WithWhitespaceString_ThrowsException")]
    public void SetHomeTeam_WithNullOrEmptyString_ThrowsException(string? homeTeam)
    {
        var builder = new FunctionApp.UpcomingEventBuilder();

        var action = () => builder.SetHomeTeam(homeTeam);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(homeTeam));
    }

    [TestCase("", TestName = "SetId_WithEmptyString_ThrowsException")]
    [TestCase(null, TestName = "SetId_WithNullString_ThrowsException")]
    [TestCase(" ", TestName = "SetId_WithWhitespaceString_ThrowsException")]
    public void SetId_WithNullOrEmptyString_ThrowsException(string? id)
    {
        var builder = new FunctionApp.UpcomingEventBuilder();

        var action = () => builder.SetId(id);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(id));
    }

    [Test]
    public void SetCommenceTime_WithNullDateTime_ThrowsException()
    {
        var builder = new FunctionApp.UpcomingEventBuilder();

        var action = () => builder.SetCommenceTime(null);

        action.Should().Throw<ArgumentNullException>().WithParameterName("commenceTime");
    }

    [Test]
    public void SetOdds_WithNullCollection_ThrowsException()
    {
        var builder = new FunctionApp.UpcomingEventBuilder();

        var action = () => builder.SetOdds(null);

        action.Should().Throw<ArgumentNullException>().WithParameterName("odds");
    }

    [Test]
    public void Setters_WithValidValues_SetProperties()
    {
        var commenceTime = new DateTime(2026, 9, 22, 18, 0, 0, DateTimeKind.Utc);
        FunctionApp.Odd[] odds = [new() { Bookmaker = "bookmaker", Away = 4.5, Draw = 3.6, Home = 1.8 }];

        var upcomingEvent = new FunctionApp.UpcomingEventBuilder()
            .SetAwayTeam("away")
            .SetHomeTeam("home")
            .SetId("id")
            .SetCommenceTime(commenceTime)
            .SetOdds(odds)
            .Instance;

        upcomingEvent.AwayTeam.Should().Be("away");
        upcomingEvent.HomeTeam.Should().Be("home");
        upcomingEvent.Id.Should().Be("id");
        upcomingEvent.CommenceTime.Should().Be(commenceTime);
        upcomingEvent.Odds.Should().BeSameAs(odds);
    }
}
