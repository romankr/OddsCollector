using OddsCollector.Functions.Models;
using OddsCollector.Functions.Tests.Data;

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
        var result = new OddBuilder().SetBookmaker(SampleEvent.Bookmaker1).Instance;

        result.Should().NotBeNull();
        result.Bookmaker.Should().NotBeNull().And.Be(SampleEvent.Bookmaker1);
    }

    [Test]
    public void SetAway_WithValidAway_ReturnsValidInstance()
    {
        var result = new OddBuilder().SetAway(SampleEvent.AwayOdd1).Instance;

        result.Should().NotBeNull();
        result.Away.Should().Be(SampleEvent.AwayOdd1);
    }

    [Test]
    public void SetDraw_WithValidDraw_ReturnsValidInstance()
    {
        var result = new OddBuilder().SetDraw(SampleEvent.DrawOdd1).Instance;

        result.Should().NotBeNull();
        result.Draw.Should().Be(SampleEvent.DrawOdd1);
    }

    [Test]
    public void SetHome_WithValidHome_ReturnsValidInstance()
    {
        var result = new OddBuilder().SetHome(SampleEvent.HomeOdd1).Instance;

        result.Should().NotBeNull();
        result.Home.Should().Be(SampleEvent.HomeOdd1);
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
