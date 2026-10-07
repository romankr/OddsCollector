using FunctionApp = OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.Tests.Tests.Predictions;

internal sealed class OutcomeScoreBuilder
{
    [TestCase("", TestName = "SetOutcome_WithEmptyString_ThrowsArgumentException")]
    [TestCase(null, TestName = "SetOutcome_WithNullString_ThrowsArgumentException")]
    [TestCase(" ", TestName = "SetOutcome_WithWhitespaceString_ThrowsArgumentException")]
    public void SetOutcome_WithNullOrEmptyString_ThrowsArgumentException(string? outcome)
    {
        var builder = new FunctionApp.OutcomeScoreBuilder();

        var action = () => builder.SetOutcome(outcome);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(outcome));
    }
}
