using FunctionApp = OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.Tests.Tests.Predictions;

internal sealed class OutcomeScore
{
    [TestCase("", TestName = "Init_WithEmptyOutcome_ThrowsArgumentException")]
    [TestCase(" ", TestName = "Init_WithWhitespaceOutcome_ThrowsArgumentException")]
    public void Init_WithBlankOutcome_ThrowsArgumentException(string outcome)
    {
        var action = () => new FunctionApp.OutcomeScore { Outcome = outcome, Score = 1 };

        action.Should().ThrowExactly<ArgumentException>().WithParameterName("Outcome");
    }
}
