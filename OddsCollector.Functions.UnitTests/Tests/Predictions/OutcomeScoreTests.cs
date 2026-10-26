using OddsCollector.Functions.Models;
using OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.UnitTests.Tests.Predictions;

internal sealed class OutcomeScoreTests
{
    [Test]
    public void Init_WithUndefinedOutcome_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        const OutcomeType undefinedOutcome = (OutcomeType)42;

        // Act
        var action = () => new OutcomeScore { Outcome = undefinedOutcome, Score = 1 };

        // Assert
        action.Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("Outcome");
    }
}
