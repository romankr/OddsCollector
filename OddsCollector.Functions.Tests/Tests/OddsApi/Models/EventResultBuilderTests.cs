using OddsCollector.Functions.Models;
using OddsCollector.Functions.Tests.Data;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Models;

internal class EventResultBuilderTests
{
    [Test]
    public void Instance_WithoutParameters_ReturnsValidInstance()
    {
        // Arrange & Act
        var eventResult = new EventResultBuilder().Instance;

        // Assert
        eventResult.Should().NotBeNull();
    }

    [Test]
    public void Instance_WithFullParameterList_ReturnsValidInstance()
    {
        // Arrange & Act
        var eventResult = new EventResultBuilder()
            .SetWinner(SampleEvent.Winner)
            .SetId(SampleEvent.Id)
            .SetCommenceTime(SampleEvent.CommenceTime)
            .SetTimestamp(SampleEvent.Timestamp)
            .SetTraceId(SampleEvent.TraceId)
            .Instance;

        // Assert
        eventResult.Should().NotBeNull().
            And.BeEquivalentTo(new EventResultBuilder().SetSampleData().Instance);
    }

    [Test]
    public void SetWinner_WithValidWinner_ReturnsValidInstance()
    {
        // Arrange & Act
        var eventResult = new EventResultBuilder().SetWinner(SampleEvent.Winner).Instance;

        // Assert
        eventResult.Should().NotBeNull();
        eventResult.Winner.Should().NotBeNull().And.Be(SampleEvent.Winner);
    }

    [Test]
    public void SetId_WithValidId_ReturnsValidInstance()
    {
        // Arrange & Act
        var eventResult = new EventResultBuilder().SetId(SampleEvent.Id).Instance;

        // Assert
        eventResult.Should().NotBeNull();
        eventResult.Id.Should().NotBeNull().And.Be(SampleEvent.Id);
    }

    [Test]
    public void SetCommenceTime_WithValidCommenceTime_ReturnsValidInstance()
    {
        // Arrange & Act
        var eventResult = new EventResultBuilder().SetCommenceTime(SampleEvent.CommenceTime).Instance;

        // Assert
        eventResult.Should().NotBeNull();
        eventResult.CommenceTime.Should().Be(SampleEvent.CommenceTime);
    }

    [Test]
    public void SetTimestamp_WithValidTimestamp_ReturnsValidInstance()
    {
        // Arrange & Act
        var eventResult = new EventResultBuilder().SetTimestamp(SampleEvent.Timestamp).Instance;

        // Assert
        eventResult.Should().NotBeNull();
        eventResult.Timestamp.Should().Be(SampleEvent.Timestamp);
    }

    [Test]
    public void SetTraceId_WithValidTraceId_ReturnsValidInstance()
    {
        // Arrange & Act
        var eventResult = new EventResultBuilder().SetTraceId(SampleEvent.TraceId).Instance;

        // Assert
        eventResult.Should().NotBeNull();
        eventResult.TraceId.Should().Be(SampleEvent.TraceId);
    }

    [Test]
    public void SetWinner_WithEmptyWinner_ThrowsException()
    {
        // Arrange & Act
        var action = () => new EventResultBuilder().SetWinner(string.Empty).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("winner");
    }

    [Test]
    public void SetWinner_WithNullWinner_ThrowsException()
    {
        // Arrange & Act
        var action = () => new EventResultBuilder().SetWinner(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("winner");
    }

    [Test]
    public void SetId_WithEmptyId_ThrowsException()
    {
        // Arrange & Act
        var action = () => new EventResultBuilder().SetId(string.Empty).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("id");
    }

    [Test]
    public void SetId_WithNullId_ThrowsException()
    {
        // Arrange & Act
        var action = () => new EventResultBuilder().SetId(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("id");
    }

    [Test]
    public void SetCommenceTime_WithNullCommenceTime_ThrowsException()
    {
        // Arrange & Act
        var action = () => new EventResultBuilder().SetCommenceTime(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("commenceTime");
    }

    [Test]
    public void SetTimestamp_WithNullTimestamp_ThrowsException()
    {
        // Arrange & Act
        var action = () => new EventResultBuilder().SetTimestamp(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("timestamp");
    }

    [Test]
    public void SetTraceId_WithNullTraceId_ThrowsException()
    {
        // Arrange & Act
        var action = () => new EventResultBuilder().SetTraceId(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("traceId");
    }
}
