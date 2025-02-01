namespace OddsCollector.Functions.Tests.Tests.Predictions;
using FunctionsApp = OddsCollector.Functions.Predictions;

internal sealed class OutcomeScoreBuilder
{
    [TestCase("", TestName = "SetScore_WithEmptyString_ThrowsException")]
    [TestCase(null, TestName = "SetScore_WithNullString_ThrowsException")]
    public void SetScore_WithNullOrEmptyString_ThrowsException(string? outcome)
    {
        var builder = new FunctionsApp.OutcomeScoreBuilder();

        var action = () => builder.SetOutcome(outcome);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(outcome));
    }
}
