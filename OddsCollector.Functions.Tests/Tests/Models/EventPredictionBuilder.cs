using FunctionApp = OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Tests.Models;

internal sealed class EventPredictionBuilder
{
    [TestCase("", TestName = "SetId_WithEmptyString_ThrowsException")]
    [TestCase(null, TestName = "SetId_WithNullString_ThrowsException")]
    public void SetId_WithNullOrEmptyString_ThrowsException(string? id)
    {
        var builder = new FunctionApp.EventPredictionBuilder();

        var action = () => builder.SetId(id);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(id));
    }

    [TestCase("", TestName = "SetAwayTeam_WithEmptyString_ThrowsException")]
    [TestCase(null, TestName = "SetAwayTeam_WithNullString_ThrowsException")]
    public void SetAwayTeam_WithNullOrEmptyString_ThrowsException(string? awayTeam)
    {
        var builder = new FunctionApp.EventPredictionBuilder();

        var action = () => builder.SetAwayTeam(awayTeam);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(awayTeam));
    }

    [TestCase("", TestName = "SetHomeTeam_WithEmptyString_ThrowsException")]
    [TestCase(null, TestName = "SetHomeTeam_WithNullString_ThrowsException")]
    public void SetHomeTeam_WithNullOrEmptyString_ThrowsException(string? homeTeam)
    {
        var builder = new FunctionApp.EventPredictionBuilder();

        var action = () => builder.SetHomeTeam(homeTeam);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(homeTeam));
    }

    [TestCase("", TestName = "SetOutcome_WithEmptyString_ThrowsException")]
    [TestCase(null, TestName = "SetOutcome_WithNullString_ThrowsException")]
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
}
