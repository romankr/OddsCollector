using FunctionApp = OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.Tests.Tests.Predictions;

internal sealed class OutcomeScoreBuilder
{
    [TestCase("", TestName = "SetOutcome_WithEmptyString_ThrowsException")]
    [TestCase(null, TestName = "SetOutcome_WithNullString_ThrowsException")]
    public void SetOutcome_WithNullOrEmptyString_ThrowsException(string? outcome)
    {
        var builder = new FunctionApp.OutcomeScoreBuilder();

        var action = () => builder.SetOutcome(outcome);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(outcome));
    }
}
