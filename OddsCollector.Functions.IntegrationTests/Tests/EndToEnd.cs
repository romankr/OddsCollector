using FluentAssertions.Execution;
using OddsCollector.Functions.Functions;
using OddsCollector.Functions.IntegrationTests.Infrastructure;
using OddsCollector.Functions.IntegrationTests.Infrastructure.OddsApi;
using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.IntegrationTests.Tests;

/// <summary>
///     Runs the functions of the app in a local Azure environment. Most tests go through one entry point:
///     UpcomingEventsFunction → Service Bus → PredictionFunction → Cosmos DB,
///     EventResultsFunction → Cosmos DB and Cosmos DB → PredictionsHttpFunction.
///     <see cref="AllFunctions_UpcomingAndCompletedEvents_PublishPredictionAndStoreResult" /> runs the whole app.
/// </summary>
/// <remarks>
///     Starting the environment takes minutes, so the tests share it. They stay independent of each other
///     and of their order: every test uses its own event ids and looks only at the documents with those ids.
/// </remarks>
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

    /// <summary>
    ///     The whole app as it runs in Azure: both timer functions collect from The Odds API and every
    ///     step after them is triggered by what the previous one produced.
    /// </summary>
    /// <remarks>
    ///     A full scenario on purpose, so it has more than one act and checks every stored and published
    ///     document. The tests above pin down each function on its own.
    /// </remarks>
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
