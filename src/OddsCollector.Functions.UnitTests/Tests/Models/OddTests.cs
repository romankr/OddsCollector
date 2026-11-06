using OddsCollector.Functions.UnitTests.Infrastructure.Models;

namespace OddsCollector.Functions.UnitTests.Tests.Models;

internal sealed class OddTests
{
    [TestCase("", TestName = "Init_WithEmptyBookmaker_ThrowsArgumentException")]
    [TestCase(" ", TestName = "Init_WithWhitespaceBookmaker_ThrowsArgumentException")]
    public void Init_WithBlankBookmaker_ThrowsArgumentException(string bookmaker)
    {
        // Arrange
        var odd = ValidModels.CreateOdd();

        // Act
        var action = () => odd with { Bookmaker = bookmaker };

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("Bookmaker");
    }
}
