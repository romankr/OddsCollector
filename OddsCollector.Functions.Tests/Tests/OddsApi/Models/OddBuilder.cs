using OddsCollector.Functions.Tests.Infrastructure.Data;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Models;

internal class OddBuilder
{
    [Test]
    public void Instance_WithoutParameters_ReturnsValidInstance()
    {
        // Arrange & Act
        var odd = new OddsCollector.Functions.Models.OddBuilder().Instance;

        // Assert
        odd.Should().NotBeNull();
    }

    [Test]
    public void Instance_WithFullParameterList_ReturnsValidInstance()
    {
        // Arrange & Act
        var odd = new OddsCollector.Functions.Models.OddBuilder()
            .SetAway(SampleEvent.AwayOdd1)
            .SetBookmaker(SampleEvent.Bookmaker1)
            .SetDraw(SampleEvent.DrawOdd1)
            .SetHome(SampleEvent.HomeOdd1)
            .Instance;

        // Assert
        odd.Should().NotBeNull().
            And.BeEquivalentTo(new OddsCollector.Functions.Models.OddBuilder().SetSampleData1().Instance);
    }

    [Test]
    public void Instance_WithValidBookmaker_ReturnsValidInstance()
    {
        // Arrange & Act
        var odd = new OddsCollector.Functions.Models.OddBuilder().SetBookmaker(SampleEvent.Bookmaker1).Instance;

        // Assert
        odd.Should().NotBeNull();
        odd.Bookmaker.Should().NotBeNull().And.Be(SampleEvent.Bookmaker1);
    }

    [Test]
    public void SetAway_WithValidAway_ReturnsValidInstance()
    {
        // Arrange & Act
        var odd = new OddsCollector.Functions.Models.OddBuilder().SetAway(SampleEvent.AwayOdd1).Instance;

        // Assert
        odd.Should().NotBeNull();
        odd.Away.Should().Be(SampleEvent.AwayOdd1);
    }

    [Test]
    public void SetDraw_WithValidDraw_ReturnsValidInstance()
    {
        // Arrange & Act
        var odd = new OddsCollector.Functions.Models.OddBuilder().SetDraw(SampleEvent.DrawOdd1).Instance;

        // Assert
        odd.Should().NotBeNull();
        odd.Draw.Should().Be(SampleEvent.DrawOdd1);
    }

    [Test]
    public void SetHome_WithValidHome_ReturnsValidInstance()
    {
        // Arrange & Act
        var odd = new OddsCollector.Functions.Models.OddBuilder().SetHome(SampleEvent.HomeOdd1).Instance;

        // Assert
        odd.Should().NotBeNull();
        odd.Home.Should().Be(SampleEvent.HomeOdd1);
    }

    [Test]
    public void SetBookmaker_WithEmptyBookmaker_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.OddBuilder().SetBookmaker(string.Empty).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("bookmaker");
    }

    [Test]
    public void SetBookmaker_WithNullBookmaker_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.OddBuilder().SetBookmaker(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("bookmaker");
    }

    [Test]
    public void SetAway_WithNullAway_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.OddBuilder().SetAway(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("away");
    }

    [Test]
    public void SetDraw_WithNullDraw_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.OddBuilder().SetDraw(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("draw");
    }

    [Test]
    public void SetHome_WithNullHome_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.OddBuilder().SetHome(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("home");
    }
}
