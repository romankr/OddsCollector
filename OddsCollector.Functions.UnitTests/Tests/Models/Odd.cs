using OddsCollector.Tests.Infrastructure.Models;

namespace OddsCollector.Functions.Tests.Tests.Models;

internal sealed class Odd
{
    [TestCase("", TestName = "Init_WithEmptyBookmaker_ThrowsArgumentException")]
    [TestCase(" ", TestName = "Init_WithWhitespaceBookmaker_ThrowsArgumentException")]
    public void Init_WithBlankBookmaker_ThrowsArgumentException(string bookmaker)
    {
        var action = () => ValidModels.CreateOdd() with { Bookmaker = bookmaker };

        action.Should().ThrowExactly<ArgumentException>().WithParameterName("Bookmaker");
    }
}
