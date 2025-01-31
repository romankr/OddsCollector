namespace OddsCollector.Functions.Tests.Tests.Models;

internal sealed class EventPredictionBuilder
{
    [TestCase("", TestName = "SetId_WithEmptyString_ThrowsException")]
    [TestCase(null, TestName = "SetId_WithNullString_ThrowsException")]
    public void SetId_WithNullOrEmptyString_ThrowsException(string? id)
    {
        var builder = new OddsCollector.Functions.Models.EventPredictionBuilder();

        var action = () => builder.SetId(id);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(id));
    }

    [TestCase("", TestName = "SetAwayTeam_WithEmptyString_ThrowsException")]
    [TestCase(null, TestName = "SetAwayTeam_WithNullString_ThrowsException")]
    public void SetAwayTeam_WithNullOrEmptyString_ThrowsException(string? awayTeam)
    {
        var builder = new OddsCollector.Functions.Models.EventPredictionBuilder();

        var action = () => builder.SetAwayTeam(awayTeam);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(awayTeam));
    }

    [TestCase("", TestName = "SetHomeTeam_WithEmptyString_ThrowsException")]
    [TestCase(null, TestName = "SetHomeTeam_WithNullString_ThrowsException")]
    public void SetHomeTeam_WithNullOrEmptyString_ThrowsException(string? homeTeam)
    {
        var builder = new OddsCollector.Functions.Models.EventPredictionBuilder();

        var action = () => builder.SetHomeTeam(homeTeam);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(homeTeam));
    }

    [TestCase("", TestName = "SetWinner_WithEmptyString_ThrowsException")]
    [TestCase(null, TestName = "SetWinner_WithNullString_ThrowsException")]
    public void SetWinner_WithNullOrEmptyString_ThrowsException(string? winner)
    {
        var builder = new OddsCollector.Functions.Models.EventPredictionBuilder();

        var action = () => builder.SetWinner(winner);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(winner));
    }

    [Test]
    public void SetCommenceTime_WithNullDateTime_ThrowsException()
    {
        var builder = new OddsCollector.Functions.Models.EventPredictionBuilder();

        var action = () => builder.SetCommenceTime(null);

        action.Should().Throw<ArgumentException>().WithParameterName("commenceTime");
    }
}
