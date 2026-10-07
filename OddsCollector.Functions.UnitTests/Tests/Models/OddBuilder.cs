using FunctionApp = OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Tests.Models;

internal sealed class OddBuilder
{
    [TestCase("", TestName = "SetBookmaker_WithEmptyString_ThrowsArgumentException")]
    [TestCase(null, TestName = "SetBookmaker_WithNullString_ThrowsArgumentException")]
    [TestCase(" ", TestName = "SetBookmaker_WithWhitespaceString_ThrowsArgumentException")]
    public void SetBookmaker_WithNullOrEmptyString_ThrowsArgumentException(string? bookmaker)
    {
        var builder = new FunctionApp.OddBuilder();

        var action = () => builder.SetBookmaker(bookmaker);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(bookmaker));
    }

    [Test]
    public void SetAway_WithNullValue_ThrowsArgumentNullException()
    {
        var builder = new FunctionApp.OddBuilder();

        var action = () => builder.SetAway(null);

        action.Should().Throw<ArgumentNullException>().WithParameterName("away");
    }

    [Test]
    public void SetDraw_WithNullValue_ThrowsArgumentNullException()
    {
        var builder = new FunctionApp.OddBuilder();

        var action = () => builder.SetDraw(null);

        action.Should().Throw<ArgumentNullException>().WithParameterName("draw");
    }

    [Test]
    public void SetHome_WithNullValue_ThrowsArgumentNullException()
    {
        var builder = new FunctionApp.OddBuilder();

        var action = () => builder.SetHome(null);

        action.Should().Throw<ArgumentNullException>().WithParameterName("home");
    }

    [Test]
    public void Setters_WithValidValues_SetProperties()
    {
        var odd = new FunctionApp.OddBuilder()
            .SetBookmaker("bookmaker")
            .SetAway(4.5)
            .SetDraw(3.6)
            .SetHome(1.8)
            .Instance;

        odd.Should().BeEquivalentTo(new FunctionApp.Odd { Bookmaker = "bookmaker", Away = 4.5, Draw = 3.6, Home = 1.8 });
    }
}
