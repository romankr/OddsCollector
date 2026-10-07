using FunctionApp = OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Tests.Models;

internal sealed class UpcomingEventBuilder
{
    [TestCase("", TestName = "SetAwayTeam_WithEmptyString_ThrowsArgumentException")]
    [TestCase(null, TestName = "SetAwayTeam_WithNullString_ThrowsArgumentException")]
    [TestCase(" ", TestName = "SetAwayTeam_WithWhitespaceString_ThrowsArgumentException")]
    public void SetAwayTeam_WithNullOrEmptyString_ThrowsArgumentException(string? awayTeam)
    {
        var builder = new FunctionApp.UpcomingEventBuilder();

        var action = () => builder.SetAwayTeam(awayTeam);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(awayTeam));
    }

    [TestCase("", TestName = "SetHomeTeam_WithEmptyString_ThrowsArgumentException")]
    [TestCase(null, TestName = "SetHomeTeam_WithNullString_ThrowsArgumentException")]
    [TestCase(" ", TestName = "SetHomeTeam_WithWhitespaceString_ThrowsArgumentException")]
    public void SetHomeTeam_WithNullOrEmptyString_ThrowsArgumentException(string? homeTeam)
    {
        var builder = new FunctionApp.UpcomingEventBuilder();

        var action = () => builder.SetHomeTeam(homeTeam);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(homeTeam));
    }

    [TestCase("", TestName = "SetId_WithEmptyString_ThrowsArgumentException")]
    [TestCase(null, TestName = "SetId_WithNullString_ThrowsArgumentException")]
    [TestCase(" ", TestName = "SetId_WithWhitespaceString_ThrowsArgumentException")]
    public void SetId_WithNullOrEmptyString_ThrowsArgumentException(string? id)
    {
        var builder = new FunctionApp.UpcomingEventBuilder();

        var action = () => builder.SetId(id);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(id));
    }

    [Test]
    public void SetCommenceTime_WithNullDateTime_ThrowsArgumentNullException()
    {
        var builder = new FunctionApp.UpcomingEventBuilder();

        var action = () => builder.SetCommenceTime(null);

        action.Should().Throw<ArgumentNullException>().WithParameterName("commenceTime");
    }

    [Test]
    public void SetOdds_WithNullCollection_ThrowsArgumentNullException()
    {
        var builder = new FunctionApp.UpcomingEventBuilder();

        var action = () => builder.SetOdds(null);

        action.Should().Throw<ArgumentNullException>().WithParameterName("odds");
    }

    [TestCase(DateTimeKind.Unspecified, TestName = "SetCommenceTime_WithUnspecifiedKind_ThrowsArgumentException")]
    [TestCase(DateTimeKind.Local, TestName = "SetCommenceTime_WithLocalKind_ThrowsArgumentException")]
    public void SetCommenceTime_WithNonUtcDateTime_ThrowsArgumentException(DateTimeKind kind)
    {
        var builder = new FunctionApp.UpcomingEventBuilder();

        var action = () => builder.SetCommenceTime(new DateTime(2026, 9, 22, 18, 0, 0, kind));

        action.Should().Throw<ArgumentException>().WithParameterName("commenceTime");
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

        upcomingEvent.Should().BeEquivalentTo(new FunctionApp.UpcomingEvent
        {
            AwayTeam = "away",
            HomeTeam = "home",
            Id = "id",
            CommenceTime = commenceTime,
            Odds = odds
        });
    }
}
