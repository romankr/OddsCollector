using FunctionApp = OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Tests.Models;

internal sealed class EventResultBuilder
{
    [TestCase("", TestName = "SetId_WithEmptyString_ThrowsArgumentException")]
    [TestCase(null, TestName = "SetId_WithNullString_ThrowsArgumentException")]
    [TestCase(" ", TestName = "SetId_WithWhitespaceString_ThrowsArgumentException")]
    public void SetId_WithNullOrEmptyString_ThrowsArgumentException(string? id)
    {
        var builder = new FunctionApp.EventResultBuilder();

        var action = () => builder.SetId(id);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(id));
    }

    [TestCase("", TestName = "SetOutcome_WithEmptyString_ThrowsArgumentException")]
    [TestCase(null, TestName = "SetOutcome_WithNullString_ThrowsArgumentException")]
    [TestCase(" ", TestName = "SetOutcome_WithWhitespaceString_ThrowsArgumentException")]
    public void SetOutcome_WithNullOrEmptyString_ThrowsArgumentException(string? outcome)
    {
        var builder = new FunctionApp.EventResultBuilder();

        var action = () => builder.SetOutcome(outcome);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(outcome));
    }

    [Test]
    public void SetCommenceTime_WithNullDateTime_ThrowsArgumentNullException()
    {
        var builder = new FunctionApp.EventResultBuilder();

        var action = () => builder.SetCommenceTime(null);

        action.Should().Throw<ArgumentNullException>().WithParameterName("commenceTime");
    }

    [TestCase(DateTimeKind.Unspecified, TestName = "SetCommenceTime_WithUnspecifiedKind_ThrowsArgumentException")]
    [TestCase(DateTimeKind.Local, TestName = "SetCommenceTime_WithLocalKind_ThrowsArgumentException")]
    public void SetCommenceTime_WithNonUtcDateTime_ThrowsArgumentException(DateTimeKind kind)
    {
        var builder = new FunctionApp.EventResultBuilder();

        var action = () => builder.SetCommenceTime(new DateTime(2026, 9, 22, 18, 0, 0, kind));

        action.Should().Throw<ArgumentException>().WithParameterName("commenceTime");
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

        result.Should().BeEquivalentTo(new FunctionApp.EventResult
        {
            Id = "id",
            Outcome = FunctionApp.OutcomeTypes.Draw,
            CommenceTime = commenceTime
        });
    }
}
