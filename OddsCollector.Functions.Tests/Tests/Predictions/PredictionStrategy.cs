﻿using FluentAssertions.Execution;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.Tests.Tests.Predictions;

internal class PredictionStrategy
{
    [Test]
    public void GetPrediction_WithUpcomingEvent_ReturnsPrediction()
    {
        // Arrange
        var expectedDateTime = DateTime.UtcNow;
        var expectedAwayTeam = "Liverpool";
        var expectedHomeTeam = "Manchester City";
        var expectedCommenceTime = new DateTime(2023, 11, 25, 12, 30, 0);
        var expectedId = Guid.NewGuid().ToString();
        var expectedTraceId = Guid.NewGuid();

        var upcomingEvent = new UpcomingEvent()
        {
            AwayTeam = expectedAwayTeam,
            CommenceTime = expectedCommenceTime,
            HomeTeam = expectedHomeTeam,
            Id = expectedId,
            TraceId = expectedTraceId
        };

        var finderStub = Substitute.For<IWinnerFinder>();
        finderStub.GetWinner(Arg.Any<ICollection<Odd>>()).Returns(expectedHomeTeam);

        var strategy = new OddsCollector.Functions.Predictions.PredictionStrategy(finderStub);

        // Act
        var prediction = strategy.GetPrediction(upcomingEvent, expectedDateTime);

        // Assert
        prediction.Should().NotBeNull();

        using (var scope = new AssertionScope())
        {
            prediction.AwayTeam.Should().NotBeNullOrEmpty().And.Be(expectedAwayTeam);
            prediction.HomeTeam.Should().NotBeNullOrEmpty().And.Be(expectedHomeTeam);
            prediction.CommenceTime.Should().Be(expectedCommenceTime);
            prediction.Id.Should().NotBeNullOrEmpty().And.Be(expectedId);
            prediction.TraceId.Should().Be(expectedTraceId);
            prediction.Winner.Should().Be(expectedHomeTeam);
            prediction.Timestamp.Should().BeCloseTo(DateTime.UtcNow, new TimeSpan(0, 0, 5));
        }
    }

    [Test]
    public void GetPrediction_WithNullUpcomingEvent_ThrowsException()
    {
        var finderStub = Substitute.For<IWinnerFinder>();

        var strategy = new OddsCollector.Functions.Predictions.PredictionStrategy(finderStub);

        var action = () => strategy.GetPrediction(null, DateTime.Now);

        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("upcomingEvent");
    }

    [Test]
    public void GetPrediction_WithNullTimestamp_ThrowsException()
    {
        var finderStub = Substitute.For<IWinnerFinder>();

        var strategy = new OddsCollector.Functions.Predictions.PredictionStrategy(finderStub);

        var action = () => strategy.GetPrediction(new UpcomingEvent(), null);

        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("timestamp");
    }
}
