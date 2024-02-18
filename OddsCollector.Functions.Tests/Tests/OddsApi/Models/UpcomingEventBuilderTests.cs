﻿using OddsCollector.Functions.Models;
using OddsCollector.Functions.Tests.Data;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Models;

internal class UpcomingEventBuilderTests
{
    [Test]
    public void Instance_WithoutParameters_ReturnsValidInstance()
    {
        // Arrange & Act
        var upcomingEvent = new UpcomingEventBuilder().Instance;

        // Assert
        upcomingEvent.Should().NotBeNull();
    }

    [Test]
    public void Instance_WithFullParameterList_ReturnsValidInstance()
    {
        // Arrange & Act
        var upcomingEvent = new UpcomingEventBuilder()
            .SetAwayTeam(SampleEvent.AwayTeam)
            .SetHomeTeam(SampleEvent.HomeTeam)
            .SetId(SampleEvent.Id)
            .SetCommenceTime(SampleEvent.CommenceTime)
            .SetTimestamp(SampleEvent.Timestamp)
            .SetTraceId(SampleEvent.TraceId)
            .SetOdds(SampleEvent.Odds)
            .Instance;

        // Assert
        upcomingEvent.Should().NotBeNull().
            And.BeEquivalentTo(new UpcomingEventBuilder().SetSampleData().Instance);
    }

    [Test]
    public void SetAwayTeam_WithValidAwayTeam_ReturnsValidInstance()
    {
        // Arrange & Act
        var upcomingEvent = new UpcomingEventBuilder().SetAwayTeam(SampleEvent.AwayTeam).Instance;

        // Assert
        upcomingEvent.Should().NotBeNull();
        upcomingEvent.AwayTeam.Should().Be(SampleEvent.AwayTeam);
    }

    [Test]
    public void SetHomeTeam_WithValidHomeTeam_ReturnsValidInstance()
    {
        // Arrange & Act
        var upcomingEvent = new UpcomingEventBuilder().SetHomeTeam(SampleEvent.HomeTeam).Instance;

        // Assert
        upcomingEvent.Should().NotBeNull();
        upcomingEvent.HomeTeam.Should().NotBeNull().And.Be(SampleEvent.HomeTeam);
    }

    [Test]
    public void SetId_WithValidId_ReturnsValidInstance()
    {
        // Arrange & Act
        var upcomingEvent = new UpcomingEventBuilder().SetId(SampleEvent.Id).Instance;

        // Assert
        upcomingEvent.Should().NotBeNull();
        upcomingEvent.Id.Should().NotBeNull().And.Be(SampleEvent.Id);
    }

    [Test]
    public void SetCommenceTime_WithValidCommenceTime_ReturnsValidInstance()
    {
        // Arrange & Act
        var upcomingEvent = new UpcomingEventBuilder().SetCommenceTime(SampleEvent.CommenceTime).Instance;

        // Assert
        upcomingEvent.Should().NotBeNull();
        upcomingEvent.CommenceTime.Should().Be(SampleEvent.CommenceTime);
    }

    [Test]
    public void SetTimestamp_WithValidTimestamp_ReturnsValidInstance()
    {
        // Arrange & Act
        var upcomingEvent = new UpcomingEventBuilder().SetTimestamp(SampleEvent.Timestamp).Instance;

        // Assert
        upcomingEvent.Should().NotBeNull();
        upcomingEvent.Timestamp.Should().Be(SampleEvent.Timestamp);
    }

    [Test]
    public void SetTraceId_WithValidTraceId_ReturnsValidInstance()
    {
        // Arrange & Act
        var upcomingEvent = new UpcomingEventBuilder().SetTraceId(SampleEvent.TraceId).Instance;

        // Assert
        upcomingEvent.Should().NotBeNull();
        upcomingEvent.TraceId.Should().Be(SampleEvent.TraceId);
    }

    [Test]
    public void SetAwayTeam_WithNullAwayTeam_ThrowsException()
    {
        // Arrange & Act
        var action = () => new UpcomingEventBuilder().SetAwayTeam(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("awayTeam");
    }

    public void SetAwayTeam_WithEmptyAwayTeam_ThrowsException()
    {
        // Arrange & Act
        var action = () => new UpcomingEventBuilder().SetAwayTeam(string.Empty).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("awayTeam");
    }

    [Test]
    public void SetHomeTeam_WithEmptyHomeTeam_ThrowsException()
    {
        // Arrange & Act
        var action = () => new UpcomingEventBuilder().SetHomeTeam(string.Empty).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("homeTeam");
    }

    [Test]
    public void SetHomeTeam_WithNullHomeTeam_ThrowsException()
    {
        // Arrange & Act
        var action = () => new UpcomingEventBuilder().SetHomeTeam(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("homeTeam");
    }

    [Test]
    public void SetId_WithEmptyId_ThrowsException()
    {
        // Arrange & Act
        var action = () => new UpcomingEventBuilder().SetId(string.Empty).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("id");
    }

    [Test]
    public void SetId_WithNullId_ThrowsException()
    {
        // Arrange & Act
        var action = () => new UpcomingEventBuilder().SetId(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("id");
    }

    [Test]
    public void SetCommenceTime_WithNullCommenceTime_ThrowsException()
    {
        // Arrange & Act
        var action = () => new UpcomingEventBuilder().SetCommenceTime(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("commenceTime");
    }

    [Test]
    public void SetTimestamp_WithNullTimestamp_ThrowsException()
    {
        // Arrange & Act
        var action = () => new UpcomingEventBuilder().SetTimestamp(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("timestamp");
    }

    [Test]
    public void SetTraceId_WithNullTraceId_ThrowsException()
    {
        // Arrange & Act
        var action = () => new UpcomingEventBuilder().SetTraceId(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("traceId");
    }

    [Test]
    public void SetOdds_WithNullOdds_ThrowsException()
    {
        // Arrange & Act
        var action = () => new UpcomingEventBuilder().SetOdds(null).Instance;

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("odds");
    }
}
