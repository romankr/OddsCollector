using FluentAssertions.Execution;
using FunctionApp = OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Tests.Models;

internal sealed class EventPredictionBuilder
{
    [TestCase("", TestName = "SetId_WithEmptyString_ThrowsArgumentException")]
    [TestCase(null, TestName = "SetId_WithNullString_ThrowsArgumentException")]
    [TestCase(" ", TestName = "SetId_WithWhitespaceString_ThrowsArgumentException")]
    public void SetId_WithNullOrEmptyString_ThrowsArgumentException(string? id)
    {
        var builder = new FunctionApp.EventPredictionBuilder();

        var action = () => builder.SetId(id);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(id));
    }

    [TestCase("", TestName = "SetAwayTeam_WithEmptyString_ThrowsArgumentException")]
    [TestCase(null, TestName = "SetAwayTeam_WithNullString_ThrowsArgumentException")]
    [TestCase(" ", TestName = "SetAwayTeam_WithWhitespaceString_ThrowsArgumentException")]
    public void SetAwayTeam_WithNullOrEmptyString_ThrowsArgumentException(string? awayTeam)
    {
        var builder = new FunctionApp.EventPredictionBuilder();

        var action = () => builder.SetAwayTeam(awayTeam);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(awayTeam));
    }

    [TestCase("", TestName = "SetHomeTeam_WithEmptyString_ThrowsArgumentException")]
    [TestCase(null, TestName = "SetHomeTeam_WithNullString_ThrowsArgumentException")]
    [TestCase(" ", TestName = "SetHomeTeam_WithWhitespaceString_ThrowsArgumentException")]
    public void SetHomeTeam_WithNullOrEmptyString_ThrowsArgumentException(string? homeTeam)
    {
        var builder = new FunctionApp.EventPredictionBuilder();

        var action = () => builder.SetHomeTeam(homeTeam);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(homeTeam));
    }

    [TestCase("", TestName = "SetOutcome_WithEmptyString_ThrowsArgumentException")]
    [TestCase(null, TestName = "SetOutcome_WithNullString_ThrowsArgumentException")]
    [TestCase(" ", TestName = "SetOutcome_WithWhitespaceString_ThrowsArgumentException")]
    public void SetOutcome_WithNullOrEmptyString_ThrowsArgumentException(string? outcome)
    {
        var builder = new FunctionApp.EventPredictionBuilder();

        var action = () => builder.SetOutcome(outcome);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(outcome));
    }

    [Test]
    public void SetCommenceTime_WithNullDateTime_ThrowsArgumentException()
    {
        var builder = new FunctionApp.EventPredictionBuilder();

        var action = () => builder.SetCommenceTime(null);

        action.Should().Throw<ArgumentException>().WithParameterName("commenceTime");
    }

    [TestCase(DateTimeKind.Unspecified, TestName = "SetCommenceTime_WithUnspecifiedKind_ThrowsArgumentException")]
    [TestCase(DateTimeKind.Local, TestName = "SetCommenceTime_WithLocalKind_ThrowsArgumentException")]
    public void SetCommenceTime_WithNonUtcDateTime_ThrowsArgumentException(DateTimeKind kind)
    {
        var builder = new FunctionApp.EventPredictionBuilder();

        var action = () => builder.SetCommenceTime(new DateTime(2026, 9, 22, 18, 0, 0, kind));

        action.Should().Throw<ArgumentException>().WithParameterName("commenceTime");
    }

    [Test]
    public void SetCommenceTime_WithUtcDateTime_SetsCommenceTime()
    {
        var commenceTime = new DateTime(2026, 9, 22, 18, 0, 0, DateTimeKind.Utc);

        var prediction = new FunctionApp.EventPredictionBuilder().SetCommenceTime(commenceTime).Instance;

        // DateTime equality ignores Kind, so it is checked on its own.
        using var scope = new AssertionScope();

        prediction.CommenceTime.Should().Be(commenceTime);
        prediction.CommenceTime.Kind.Should().Be(DateTimeKind.Utc);
    }
}
