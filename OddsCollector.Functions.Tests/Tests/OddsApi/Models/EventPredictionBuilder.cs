using OddsCollector.Functions.Tests.Infrastructure.Data;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Models;

internal class EventPredictionBuilder
{
    [Test]
    public void Instance_WithoutParameters_ReturnsValidInstance()
    {
        // Arrange & Act
        var eventPrediction = new OddsCollector.Functions.Models.EventPredictionBuilder().Instance;

        // Assert
        eventPrediction.Should().NotBeNull();
    }

    [Test]
    public void Instance_WithFullParameterList_ReturnsValidInstance()
    {
        // Arrange & Act
        var eventPrediction = new OddsCollector.Functions.Models.EventPredictionBuilder()
            .SetBookmaker(SampleEvent.Bookmaker1)
            .SetWinner(SampleEvent.Winner)
            .SetAwayTeam(SampleEvent.AwayTeam)
            .SetHomeTeam(SampleEvent.HomeTeam)
            .SetId(SampleEvent.Id)
            .SetStrategy(SampleEvent.Strategy)
            .SetCommenceTime(SampleEvent.CommenceTime)
            .SetTimestamp(SampleEvent.Timestamp)
            .SetTraceId(SampleEvent.TraceId)
            .Instance;

        // Assert
        eventPrediction.Should().NotBeNull().
            And.BeEquivalentTo(new OddsCollector.Functions.Models.EventPredictionBuilder().SetSampleData().Instance);
    }

    [Test]
    public void SetBookmaker_WithValidBookmaker_ReturnsValidInstance()
    {
        // Arrange & Act
        var eventPrediction = new OddsCollector.Functions.Models.EventPredictionBuilder().SetBookmaker(SampleEvent.Bookmaker1).Instance;

        // Assert
        eventPrediction.Should().NotBeNull();
        eventPrediction.Bookmaker.Should().NotBeNull().And.Be(SampleEvent.Bookmaker1);
    }

    [Test]
    public void SetWinner_WithValidWinner_ReturnsValidInstance()
    {
        // Arrange & Act
        var eventPrediction = new OddsCollector.Functions.Models.EventPredictionBuilder().SetWinner(SampleEvent.Winner).Instance;

        // Assert
        eventPrediction.Should().NotBeNull();
        eventPrediction.Winner.Should().NotBeNull().And.Be(SampleEvent.Winner);
    }

    [Test]
    public void SetAwayTeam_WithValidAwayTeam_ReturnsValidInstance()
    {
        // Arrange & Act
        var eventPrediction = new OddsCollector.Functions.Models.EventPredictionBuilder().SetAwayTeam(SampleEvent.AwayTeam).Instance;

        // Assert
        eventPrediction.Should().NotBeNull();
        eventPrediction.AwayTeam.Should().NotBeNull().And.Be(SampleEvent.AwayTeam);
    }

    [Test]
    public void SetHomeTeam_WithValidHomeTeam_ReturnsValidInstance()
    {
        // Arrange & Act
        var eventPrediction = new OddsCollector.Functions.Models.EventPredictionBuilder().SetHomeTeam(SampleEvent.HomeTeam).Instance;

        // Assert
        eventPrediction.Should().NotBeNull();
        eventPrediction.HomeTeam.Should().NotBeNull().And.Be(SampleEvent.HomeTeam);
    }

    [Test]
    public void SetId_WithValidId_ReturnsValidInstance()
    {
        // Arrange & Act
        var eventPrediction = new OddsCollector.Functions.Models.EventPredictionBuilder().SetId(SampleEvent.Id).Instance;

        // Assert
        eventPrediction.Should().NotBeNull();
        eventPrediction.Id.Should().NotBeNull().And.Be(SampleEvent.Id);
    }

    [Test]
    public void SetStrategy_WithValidStrategy_ReturnsValidInstance()
    {
        // Arrange & Act
        var eventPrediction = new OddsCollector.Functions.Models.EventPredictionBuilder().SetStrategy(SampleEvent.Strategy).Instance;

        // Assert
        eventPrediction.Should().NotBeNull();
        eventPrediction.Strategy.Should().NotBeNull().And.Be(SampleEvent.Strategy);
    }

    [Test]
    public void SetCommenceTime_WithValidCommenceTime_ReturnsValidInstance()
    {
        // Arrange & Act
        var eventPrediction = new OddsCollector.Functions.Models.EventPredictionBuilder().SetCommenceTime(SampleEvent.CommenceTime).Instance;

        // Assert
        eventPrediction.Should().NotBeNull();
        eventPrediction.CommenceTime.Should().Be(SampleEvent.CommenceTime);
    }

    [Test]
    public void SetTimestamp_WithValidTimestamp_ReturnsValidInstance()
    {
        // Arrange & Act
        var eventPrediction = new OddsCollector.Functions.Models.EventPredictionBuilder().SetTimestamp(SampleEvent.Timestamp).Instance;

        // Assert
        eventPrediction.Should().NotBeNull();
        eventPrediction.Timestamp.Should().Be(SampleEvent.Timestamp);
    }

    [Test]
    public void SetTraceId_WithValidTraceId_ReturnsValidInstance()
    {
        // Arrange & Act
        var eventPrediction = new OddsCollector.Functions.Models.EventPredictionBuilder().SetTraceId(SampleEvent.TraceId).Instance;

        // Assert
        eventPrediction.Should().NotBeNull();
        eventPrediction.TraceId.Should().Be(SampleEvent.TraceId);
    }

    [Test]
    public void SetBookmaker_WithEmptyBookmaker_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.EventPredictionBuilder().SetBookmaker(string.Empty).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("bookmaker");
    }

    [Test]
    public void SetBookmaker_WithNullBookmaker_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.EventPredictionBuilder().SetBookmaker(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("bookmaker");
    }

    [Test]
    public void SetWinner_WithEmptyWinner_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.EventPredictionBuilder().SetWinner(string.Empty).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("winner");
    }

    [Test]
    public void SetWinner_WithNullWinner_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.EventPredictionBuilder().SetWinner(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("winner");
    }

    [Test]
    public void SetAwayTeam_WithNullAwayTeam_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.EventPredictionBuilder().SetAwayTeam(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("awayTeam");
    }

    [Test]
    public void SetAwayTeam_WithNullOrEmptyAwayTeam_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.EventPredictionBuilder().SetAwayTeam(string.Empty).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("awayTeam");
    }

    [Test]
    public void SetHomeTeam_WithEmptyHomeTeam_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.EventPredictionBuilder().SetHomeTeam(string.Empty).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("homeTeam");
    }

    [Test]
    public void SetHomeTeam_WithNullHomeTeam_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.EventPredictionBuilder().SetHomeTeam(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("homeTeam");
    }

    [Test]
    public void SetId_WithEmptyId_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.EventPredictionBuilder().SetId(string.Empty).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("id");
    }

    [Test]
    public void SetId_WithNullId_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.EventPredictionBuilder().SetId(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("id");
    }

    [Test]
    public void SetStrategy_WithEmptyStrategy_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.EventPredictionBuilder().SetStrategy(string.Empty).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("strategy");
    }

    [Test]
    public void SetStrategy_WithNullStrategy_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.EventPredictionBuilder().SetStrategy(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("strategy");
    }

    [Test]
    public void SetCommenceTime_WithNullCommenceTime_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.EventPredictionBuilder().SetCommenceTime(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("commenceTime");
    }

    [Test]
    public void SetTimestamp_WithNullTimestamp_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.EventPredictionBuilder().SetTimestamp(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("timestamp");
    }

    [Test]
    public void SetTraceId_WithNullTraceId_ThrowsException()
    {
        // Arrange & Act
        var action = () => new OddsCollector.Functions.Models.EventPredictionBuilder().SetTraceId(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("traceId");
    }
}
