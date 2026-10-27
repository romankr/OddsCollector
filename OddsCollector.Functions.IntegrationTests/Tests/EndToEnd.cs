using System.Net;
using System.Text.Json;
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

    private static readonly TimeSpan SettlementGracePeriod = TimeSpan.FromSeconds(5);

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
            Outcome = OutcomeType.HomeTeam
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
            Outcome = OutcomeType.HomeTeam
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
            Outcome = OutcomeType.HomeTeam
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
                Outcome = OutcomeType.HomeTeam
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
            Outcome = OutcomeType.HomeTeam
        };

        using (new AssertionScope())
        {
            publishedPrediction.Should().BeEquivalentTo(expectedPrediction);
            storedPrediction.Should().BeEquivalentTo(expectedPrediction);
            storedResult.Should().BeEquivalentTo(new EventResult
            {
                Id = completedEvent.Id,
                CommenceTime = completedEvent.CommenceTime,
                Outcome = OutcomeType.HomeTeam
            });
        }
    }

    [Test]
    [CancelAfter(TestTimeoutMilliseconds)]
    public async Task PredictionFunction_UnreadableMessage_DeadLettersItWithoutSettlingItAgain(
        CancellationToken cancellationToken)
    {
        // Arrange
        var messageId = TestData.NewId("unreadable");
        const string body = """{"Id":"unreadable"}""";

        var logStart = Environment.FunctionsHost.LoggedLines;

        // Act
        await Environment.ServiceBusQueue.SendAsync(messageId, body, cancellationToken);

        var deadLettered = await Environment.WaitForDeadLetteredMessageAsync(messageId, cancellationToken);
        var executed = await Environment.WaitForLogLineAsync(logStart,
            $"Executed 'Functions.{nameof(PredictionFunction)}'", cancellationToken);

        await Task.Delay(SettlementGracePeriod, cancellationToken);

        // Assert
        var logLines = Environment.FunctionsHost.GetLogLinesAfter(logStart);

        using var scope = new AssertionScope();

        deadLettered.DeadLetterReason.Should().Be("JsonException");
        executed.Should().Contain("Succeeded");
        logLines.Where(l => l.Contains($"MessageId: {messageId},", StringComparison.Ordinal)).Should()
            .ContainSingle().Which.Should().Contain("DeliveryCount: 1,");
        logLines.Should().NotContain(l => l.Contains("Message processing error", StringComparison.Ordinal));
    }

    [Test]
    [CancelAfter(TestTimeoutMilliseconds)]
    public async Task PredictionFunction_EventWithoutUsableOdds_CompletesMessageWithoutPrediction(
        CancellationToken cancellationToken)
    {
        // Arrange
        var messageId = TestData.NewId("unpredictable");
        var upcomingEvent = new UpcomingEvent
        {
            Id = TestData.NewId("unpredictable"),
            HomeTeam = "Arsenal",
            AwayTeam = "Chelsea",
            CommenceTime = TestData.KickOffInDays(2),
            Odds = [new Odd { Bookmaker = "bookmaker1", Home = 1.0, Draw = 1.0, Away = 1.0 }]
        };

        var logStart = Environment.FunctionsHost.LoggedLines;

        // Act
        await Environment.ServiceBusQueue.SendAsync(messageId, JsonSerializer.Serialize(upcomingEvent),
            cancellationToken);

        var executed = await Environment.WaitForLogLineAsync(logStart,
            $"Executed 'Functions.{nameof(PredictionFunction)}'", cancellationToken);

        await Task.Delay(SettlementGracePeriod, cancellationToken);

        var pending = await Environment.ServiceBusQueue.TryPeekAsync(messageId, cancellationToken);
        var deadLettered = await Environment.ServiceBusQueue.TryPeekDeadLetteredAsync(messageId, cancellationToken);
        var storedPrediction = await Environment.TryReadPredictionAsync(upcomingEvent.Id, cancellationToken);

        // Assert
        var logLines = Environment.FunctionsHost.GetLogLinesAfter(logStart);

        using var scope = new AssertionScope();

        executed.Should().Contain("Succeeded");
        pending.Should().BeNull();
        deadLettered.Should().BeNull();
        storedPrediction.Should().BeNull();
        logLines.Where(l => l.Contains($"MessageId: {messageId},", StringComparison.Ordinal)).Should()
            .ContainSingle().Which.Should().Contain("DeliveryCount: 1,");
    }

    [Test]
    [CancelAfter(TestTimeoutMilliseconds)]
    public async Task PredictionFunction_EventFailingValidation_DeadLettersIt(CancellationToken cancellationToken)
    {
        // Arrange
        var messageId = TestData.NewId("invalid");
        var invalidEvent = new
        {
            Id = TestData.NewId("invalid"),
            HomeTeam = " ",
            AwayTeam = "Chelsea",
            CommenceTime = TestData.KickOffInDays(2),
            Odds = new[] { new { Bookmaker = "bookmaker1", Home = 1.5, Draw = 4.0, Away = 6.0 } }
        };

        var logStart = Environment.FunctionsHost.LoggedLines;

        // Act
        await Environment.ServiceBusQueue.SendAsync(messageId, JsonSerializer.Serialize(invalidEvent),
            cancellationToken);

        var deadLettered = await Environment.WaitForDeadLetteredMessageAsync(messageId, cancellationToken);
        var executed = await Environment.WaitForLogLineAsync(logStart,
            $"Executed 'Functions.{nameof(PredictionFunction)}'", cancellationToken);

        var storedPrediction = await Environment.TryReadPredictionAsync(invalidEvent.Id, cancellationToken);

        // Assert
        var logLines = Environment.FunctionsHost.GetLogLinesAfter(logStart);

        using var scope = new AssertionScope();

        deadLettered.DeadLetterReason.Should().Be(nameof(ArgumentException));
        deadLettered.DeadLetterErrorDescription.Should().StartWith("HomeTeam cannot be empty");
        executed.Should().Contain("Succeeded");
        storedPrediction.Should().BeNull();
        logLines.Where(l => l.Contains($"MessageId: {messageId},", StringComparison.Ordinal)).Should()
            .ContainSingle().Which.Should().Contain("DeliveryCount: 1,");
    }

    [Test]
    [CancelAfter(TestTimeoutMilliseconds)]
    public async Task PredictionsHttpFunction_StoredDocumentFailingValidation_ReturnsTheOtherPredictions(
        CancellationToken cancellationToken)
    {
        // Arrange: a document with an Outcome the model does not know, stored before a valid prediction.
        var brokenId = TestData.NewId("broken");
        var brokenDocument = new
        {
            id = brokenId,
            HomeTeam = "Arsenal",
            AwayTeam = "Chelsea",
            CommenceTime = TestData.KickOffInDays(2),
            Outcome = "Unknown"
        };

        var prediction = new EventPrediction
        {
            Id = TestData.NewId("prediction"),
            HomeTeam = "Liverpool",
            AwayTeam = "Everton",
            CommenceTime = TestData.KickOffInDays(3),
            Outcome = OutcomeType.Draw
        };

        try
        {
            await Environment.StorePredictionDocumentAsync(brokenId, brokenDocument, cancellationToken);
            await Environment.StorePredictionAsync(prediction, cancellationToken);

            // Act
            var publishedPredictions =
                await Environment.WaitForPublishedPredictionsAsync(prediction.Id, cancellationToken);

            // Assert: the response is still OK, with the valid prediction and without the unreadable document.
            using var scope = new AssertionScope();

            publishedPredictions.Should().ContainSingle(p => p.Id == prediction.Id)
                .Which.Should().BeEquivalentTo(prediction);
            publishedPredictions.Should().NotContain(p => p.Id == brokenId);
        }
        finally
        {
            await Environment.DeletePredictionAsync(brokenId, CancellationToken.None);
            await Environment.DeletePredictionAsync(prediction.Id, CancellationToken.None);
        }
    }

    [Test]
    [CancelAfter(TestTimeoutMilliseconds)]
    public async Task UpcomingEventsFunction_OddsApiUnavailable_RetriesRequestsButNotTheRun(
        CancellationToken cancellationToken)
    {
        // Arrange
        Environment.OddsApi.FailUpcomingEvents(HttpStatusCode.ServiceUnavailable);

        var requestsBefore = Environment.OddsApi.CountUpcomingEventsRequests();
        var logStart = Environment.FunctionsHost.LoggedLines;

        try
        {
            // Act
            await Environment.FunctionsHost.InvokeAsync(nameof(UpcomingEventsFunction), cancellationToken);

            var executed = await Environment.WaitForLogLineAsync(logStart,
                $"Executed 'Functions.{nameof(UpcomingEventsFunction)}'", cancellationToken);

            // A host retry of the whole run would be scheduled right after the failed execution is logged.
            await Task.Delay(SettlementGracePeriod, cancellationToken);

            var requests = Environment.OddsApi.CountUpcomingEventsRequests() - requestsBefore;

            // Assert: the HTTP client retries the request (at most three times, fewer if its 30 second total
            // timeout runs out first), and the failed run is not retried by the host on top of that.
            using var scope = new AssertionScope();

            executed.Should().Contain("Failed");
            requests.Should().BeInRange(2, 4);
            Environment.FunctionsHost.GetLogLinesAfter(logStart).Should()
                .NotContain(l => l.Contains("before retrying function execution", StringComparison.Ordinal));
        }
        finally
        {
            Environment.OddsApi.SetUpcomingEvents([]);
        }
    }
}
