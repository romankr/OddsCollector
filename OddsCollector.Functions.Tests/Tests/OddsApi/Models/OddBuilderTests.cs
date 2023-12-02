using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Models;

[Parallelizable(ParallelScope.All)]
internal class OddBuilderTests
{
    [Test]
    public void Instance_WithoutParameters_ReturnsValidInstance()
    {
        var result = new OddBuilder().Instance;

        result.Should().NotBeNull();
    }

    [Test]
    public void Instance_WithValidBookmaker_ReturnsValidInstance()
    {
        const string bookmaker = nameof(bookmaker);

        var result = new OddBuilder().SetBookmaker(bookmaker).Instance;

        result.Should().NotBeNull();
        result.Bookmaker.Should().NotBeNull().And.Be(bookmaker);
    }

    [Test]
    public void SetAway_WithValidAway_ReturnsValidInstance()
    {
        const double away = 1.1;

        var result = new OddBuilder().SetAway(away).Instance;

        result.Should().NotBeNull();
        result.Away.Should().Be(away);
    }

    [Test]
    public void SetDraw_WithValidDraw_ReturnsValidInstance()
    {
        const double draw = 1.1;

        var result = new OddBuilder().SetDraw(draw).Instance;

        result.Should().NotBeNull();
        result.Draw.Should().Be(draw);
    }

    [Test]
    public void SetHome_WithValidHome_ReturnsValidInstance()
    {
        const double home = 1.1;

        var result = new OddBuilder().SetHome(home).Instance;

        result.Should().NotBeNull();
        result.Home.Should().Be(home);
    }

    [TestCase("")]
    [TestCase(null)]
    public void SetBookmaker_WithNullOrEmptyBookmaker_ThrowsException(string? bookmaker)
    {
        var action = () =>
        {
            _ = new OddBuilder().SetBookmaker(bookmaker).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(bookmaker));
    }

    [Test]
    public void SetAway_WithNullAway_ThrowsException()
    {
        var action = () =>
        {
            _ = new OddBuilder().SetAway(null).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName("away");
    }

    [Test]
    public void SetDraw_WithNullDraw_ThrowsException()
    {
        var action = () =>
        {
            _ = new OddBuilder().SetDraw(null).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName("draw");
    }

    [Test]
    public void SetHome_WithNullHome_ThrowsException()
    {
        var action = () =>
        {
            _ = new OddBuilder().SetHome(null).Instance;
        };

        action.Should().Throw<ArgumentException>().WithParameterName("home");
    }
}
