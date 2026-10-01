using FunctionApp = OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Tests.Models;

internal sealed class EventPredictionBuilder
{
    [TestCase("", TestName = "SetId_WithEmptyString_ThrowsException")]
    [TestCase(null, TestName = "SetId_WithNullString_ThrowsException")]
    [TestCase(" ", TestName = "SetId_WithWhitespaceString_ThrowsException")]
    public void SetId_WithNullOrEmptyString_ThrowsException(string? id)
    {
        var builder = new FunctionApp.EventPredictionBuilder();

        var action = () => builder.SetId(id);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(id));
    }

    [TestCase("", TestName = "SetAwayTeam_WithEmptyString_ThrowsException")]
    [TestCase(null, TestName = "SetAwayTeam_WithNullString_ThrowsException")]
    [TestCase(" ", TestName = "SetAwayTeam_WithWhitespaceString_ThrowsException")]
    public void SetAwayTeam_WithNullOrEmptyString_ThrowsException(string? awayTeam)
    {
        var builder = new FunctionApp.EventPredictionBuilder();

        var action = () => builder.SetAwayTeam(awayTeam);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(awayTeam));
    }

    [TestCase("", TestName = "SetHomeTeam_WithEmptyString_ThrowsException")]
    [TestCase(null, TestName = "SetHomeTeam_WithNullString_ThrowsException")]
    [TestCase(" ", TestName = "SetHomeTeam_WithWhitespaceString_ThrowsException")]
    public void SetHomeTeam_WithNullOrEmptyString_ThrowsException(string? homeTeam)
    {
        var builder = new FunctionApp.EventPredictionBuilder();

        var action = () => builder.SetHomeTeam(homeTeam);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(homeTeam));
    }

    [TestCase("", TestName = "SetOutcome_WithEmptyString_ThrowsException")]
    [TestCase(null, TestName = "SetOutcome_WithNullString_ThrowsException")]
    [TestCase(" ", TestName = "SetOutcome_WithWhitespaceString_ThrowsException")]
    public void SetOutcome_WithNullOrEmptyString_ThrowsException(string? outcome)
    {
        var builder = new FunctionApp.EventPredictionBuilder();

        var action = () => builder.SetOutcome(outcome);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(outcome));
    }

    [Test]
    public void SetCommenceTime_WithNullDateTime_ThrowsException()
    {
        var builder = new FunctionApp.EventPredictionBuilder();

        var action = () => builder.SetCommenceTime(null);

        action.Should().Throw<ArgumentException>().WithParameterName("commenceTime");
    }

    [TestCase(DateTimeKind.Unspecified, TestName = "SetCommenceTime_WithUnspecifiedKind_ThrowsException")]
    [TestCase(DateTimeKind.Local, TestName = "SetCommenceTime_WithLocalKind_ThrowsException")]
    public void SetCommenceTime_WithNonUtcDateTime_ThrowsException(DateTimeKind kind)
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

        prediction.CommenceTime.Should().Be(commenceTime);
        prediction.CommenceTime.Kind.Should().Be(DateTimeKind.Utc);
    }
}
