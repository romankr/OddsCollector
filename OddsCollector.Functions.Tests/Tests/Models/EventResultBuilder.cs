using FunctionApp = OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Tests.Models;

internal sealed class EventResultBuilder
{
    [TestCase("", TestName = "SetId_WithEmptyString_ThrowsException")]
    [TestCase(null, TestName = "SetId_WithNullString_ThrowsException")]
    [TestCase(" ", TestName = "SetId_WithWhitespaceString_ThrowsException")]
    public void SetId_WithNullOrEmptyString_ThrowsException(string? id)
    {
        var builder = new FunctionApp.EventResultBuilder();

        var action = () => builder.SetId(id);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(id));
    }

    [TestCase("", TestName = "SetOutcome_WithEmptyString_ThrowsException")]
    [TestCase(null, TestName = "SetOutcome_WithNullString_ThrowsException")]
    [TestCase(" ", TestName = "SetOutcome_WithWhitespaceString_ThrowsException")]
    public void SetOutcome_WithNullOrEmptyString_ThrowsException(string? outcome)
    {
        var builder = new FunctionApp.EventResultBuilder();

        var action = () => builder.SetOutcome(outcome);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(outcome));
    }

    [Test]
    public void SetCommenceTime_WithNullDateTime_ThrowsException()
    {
        var builder = new FunctionApp.EventResultBuilder();

        var action = () => builder.SetCommenceTime(null);

        action.Should().Throw<ArgumentNullException>().WithParameterName("commenceTime");
    }

    [Test]
    public void Setters_WithValidValues_SetProperties()
    {
        var commenceTime = new DateTime(2026, 9, 22, 18, 0, 0, DateTimeKind.Utc);

        var result = new FunctionApp.EventResultBuilder()
            .SetId("id")
            .SetOutcome(FunctionApp.OutcomeTypes.Draw)
            .SetCommenceTime(commenceTime)
            .Instance;

        result.Id.Should().Be("id");
        result.Outcome.Should().Be(FunctionApp.OutcomeTypes.Draw);
        result.CommenceTime.Should().Be(commenceTime);
    }
}
