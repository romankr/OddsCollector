using FluentAssertions.Execution;
using OddsCollector.Functions.Functions;
using OddsCollector.Functions.IntegrationTests.Infrastructure;
using OddsCollector.Functions.IntegrationTests.Infrastructure.OddsApi;
using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.IntegrationTests.Tests;

[Category("Integration")]
[NonParallelizable]
internal sealed class EndToEnd
{
    private const int TestTimeoutMilliseconds = 5 * 60 * 1000;

    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(10);

    private IntegrationEnvironment? _environment;

    private IntegrationEnvironment Environment =>
        _environment ?? throw new InvalidOperationException("The environment is not started");

    [OneTimeSetUp]
    public async Task StartEnvironmentAsync()
    {
        using var startup = new CancellationTokenSource(StartupTimeout);

        _environment = await IntegrationEnvironment.StartAsync(startup.Token);
    }

    [OneTimeTearDown]
    public async Task StopEnvironmentAsync()
    {
        if (_environment is not null)
        {
            await _environment.DisposeAsync();
        }
    }

    [Test]
    [CancelAfter(TestTimeoutMilliseconds)]
    public async Task UpcomingEventsFunction_HomeTeamFavouredByEveryBookmaker_StoresHomeTeamPrediction(
        CancellationToken cancellationToken)
    {
        // Arrange
        var upcomingEvent = new OddsApiEvent(
            TestData.NewId("upcoming"),
            TestData.KickOffInDays(2),
            "Arsenal",
            "Chelsea",
            [
                new OddsApiBookmaker("bookmaker1", Home: 1.5, Away: 6.0, Draw: 4.0),
                new OddsApiBookmaker("bookmaker2", Home: 1.55, Away: 5.5, Draw: 4.2)
            ]);

        Environment.OddsApi.SetUpcomingEvents([upcomingEvent]);

        // Act
        await Environment.FunctionsHost.InvokeAsync(nameof(UpcomingEventsFunction), cancellationToken);

        // Assert
        var storedPrediction = await Environment.WaitForStoredPredictionAsync(upcomingEvent.Id, cancellationToken);

        storedPrediction.Should().BeEquivalentTo(new EventPrediction
        {
            Id = upcomingEvent.Id,
            HomeTeam = upcomingEvent.HomeTeam,
            AwayTeam = upcomingEvent.AwayTeam,
            CommenceTime = upcomingEvent.CommenceTime,
            Outcome = OutcomeTypes.HomeTeam
        });
    }

    [Test]
    [CancelAfter(TestTimeoutMilliseconds)]
    public async Task EventResultsFunction_CompletedEventWonByHomeTeam_StoresHomeTeamAsOutcome(
        CancellationToken cancellationToken)
    {
        // Arrange
        var completedEvent = new OddsApiCompletedEvent(
            TestData.NewId("completed"),
            TestData.KickOffInDays(-1),
            "Liverpool",
            "Everton",
            HomeScore: 2,
            AwayScore: 1);

        Environment.OddsApi.SetCompletedEvents([completedEvent]);

        // Act
        await Environment.FunctionsHost.InvokeAsync(nameof(EventResultsFunction), cancellationToken);

        // Assert
        var storedResult = await Environment.WaitForStoredResultAsync(completedEvent.Id, cancellationToken);

        storedResult.Should().BeEquivalentTo(new EventResult
        {
            Id = completedEvent.Id,
            CommenceTime = completedEvent.CommenceTime,
            Outcome = completedEvent.HomeTeam
        });
    }

    [Test]
    [CancelAfter(TestTimeoutMilliseconds)]
    public async Task PredictionsHttpFunction_StoredPredictionForUpcomingEvent_ReturnsPrediction(
        CancellationToken cancellationToken)
    {
        // Arrange
        var prediction = new EventPrediction
        {
            Id = TestData.NewId("prediction"),
            HomeTeam = "Arsenal",
            AwayTeam = "Chelsea",
            CommenceTime = TestData.KickOffInDays(2),
            Outcome = OutcomeTypes.HomeTeam
        };

        await Environment.StorePredictionAsync(prediction, cancellationToken);

        // Act
        var publishedPrediction = await Environment.WaitForPublishedPredictionAsync(prediction.Id, cancellationToken);

        // Assert
        publishedPrediction.Should().BeEquivalentTo(prediction);
    }

    [Test]
    [CancelAfter(TestTimeoutMilliseconds)]
    public async Task PredictionsHttpFunction_MoreThanHundredUpcomingPredictions_ReturnsHundredNearest(
        CancellationToken cancellationToken)
    {
        // Arrange
        const int returnedCount = 100;

        var firstKickOff = TestData.KickOffInHours(2);

        var predictions = Enumerable.Range(0, returnedCount + 1)
            .Select(minutes => new EventPrediction
            {
                Id = TestData.NewId("nearest"),
                HomeTeam = "Arsenal",
                AwayTeam = "Chelsea",
                CommenceTime = firstKickOff.AddMinutes(minutes),
                Outcome = OutcomeTypes.HomeTeam
            })
            .ToList();

        try
        {
            foreach (var prediction in Enumerable.Reverse(predictions))
            {
                await Environment.StorePredictionAsync(prediction, cancellationToken);
            }

            // Act
            var publishedPredictions =
                await Environment.WaitForPublishedPredictionsAsync(predictions[0].Id, cancellationToken);

            // Assert.
            publishedPredictions.Select(p => p.Id).Should()
                .Equal(predictions.Take(returnedCount).Select(p => p.Id));
        }
        finally
        {
            // Not the test's token: the cleanup has to run even when the test timed out.
            foreach (var prediction in predictions)
            {
                await Environment.DeletePredictionAsync(prediction.Id, CancellationToken.None);
            }
        }
    }

    [Test]
    [CancelAfter(TestTimeoutMilliseconds)]
    public async Task AllFunctions_UpcomingAndCompletedEvents_PublishPredictionAndStoreResult(
        CancellationToken cancellationToken)
    {
        // Arrange
        var upcomingEvent = new OddsApiEvent(
            TestData.NewId("upcoming"),
            TestData.KickOffInDays(2),
            "Arsenal",
            "Chelsea",
            [
                new OddsApiBookmaker("bookmaker1", Home: 1.5, Away: 6.0, Draw: 4.0),
                new OddsApiBookmaker("bookmaker2", Home: 1.55, Away: 5.5, Draw: 4.2)
            ]);

        var completedEvent = new OddsApiCompletedEvent(
            TestData.NewId("completed"),
            TestData.KickOffInDays(-1),
            "Liverpool",
            "Everton",
            HomeScore: 2,
            AwayScore: 1);

        Environment.OddsApi.SetUpcomingEvents([upcomingEvent]);
        Environment.OddsApi.SetCompletedEvents([completedEvent]);

        // Act
        await Environment.FunctionsHost.InvokeAsync(nameof(UpcomingEventsFunction), cancellationToken);
        await Environment.FunctionsHost.InvokeAsync(nameof(EventResultsFunction), cancellationToken);

        var publishedPrediction =
            await Environment.WaitForPublishedPredictionAsync(upcomingEvent.Id, cancellationToken);
        var storedPrediction = await Environment.WaitForStoredPredictionAsync(upcomingEvent.Id, cancellationToken);
        var storedResult = await Environment.WaitForStoredResultAsync(completedEvent.Id, cancellationToken);

        // Assert
        var expectedPrediction = new EventPrediction
        {
            Id = upcomingEvent.Id,
            HomeTeam = upcomingEvent.HomeTeam,
            AwayTeam = upcomingEvent.AwayTeam,
            CommenceTime = upcomingEvent.CommenceTime,
            Outcome = OutcomeTypes.HomeTeam
        };

        using (new AssertionScope())
        {
            publishedPrediction.Should().BeEquivalentTo(expectedPrediction);
            storedPrediction.Should().BeEquivalentTo(expectedPrediction);
            storedResult.Should().BeEquivalentTo(new EventResult
            {
                Id = completedEvent.Id,
                CommenceTime = completedEvent.CommenceTime,
                Outcome = completedEvent.HomeTeam
            });
        }
    }
}
