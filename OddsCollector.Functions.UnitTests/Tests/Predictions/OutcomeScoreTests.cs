using OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.UnitTests.Tests.Predictions;

internal sealed class OutcomeScoreTests
{
    [TestCase("", TestName = "Init_WithEmptyOutcome_ThrowsArgumentException")]
    [TestCase(" ", TestName = "Init_WithWhitespaceOutcome_ThrowsArgumentException")]
    public void Init_WithBlankOutcome_ThrowsArgumentException(string outcome)
    {
        // Act
        var action = () => new OutcomeScore { Outcome = outcome, Score = 1 };

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("Outcome");
    }
}
