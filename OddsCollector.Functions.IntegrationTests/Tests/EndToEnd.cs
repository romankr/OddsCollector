using System.Net;
using System.Net.Http.Json;
using FluentAssertions.Execution;
using OddsCollector.Functions.IntegrationTests.Infrastructure;
using OddsCollector.Functions.IntegrationTests.Infrastructure.OddsApi;
using OddsCollector.Functions.IntegrationTests.Infrastructure.Polling;
using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.IntegrationTests.Tests;

/// <summary>
///     Runs every function of the app in a local Azure environment:
///     UpcomingEventsFunction → Service Bus → PredictionFunction → Cosmos DB → PredictionsHttpFunction,
///     and EventResultsFunction → Cosmos DB.
/// </summary>
[Category("Integration")]
[NonParallelizable]
internal sealed class EndToEnd
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(2);

    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(10);

    [Test]
    public async Task AllFunctions_WithOddsApiData_PublishPredictionsAndStoreResults()
    {
        // The environment belongs to the test: started here and stopped when the test ends, pass or fail.
        using var startup = new CancellationTokenSource(StartupTimeout);
        await using var environment = await IntegrationEnvironment.StartAsync(startup.Token);

        using var test = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        var cancellationToken = test.Token;

        // Arrange: one upcoming match where every bookmaker favours the home team, one completed match.
        var now = DateTime.UtcNow;
        var commenceTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc).AddDays(2);
        var upcomingEvent = new OddsApiEvent(
            $"upcoming-{Guid.NewGuid():N}",
            commenceTime,
            "Arsenal",
            "Chelsea",
            [
                new OddsApiBookmaker("bookmaker1", 1.5, 6.0, 4.0),
                new OddsApiBookmaker("bookmaker2", 1.55, 5.5, 4.2)
            ]);

        var completedEvent = new OddsApiCompletedEvent(
            $"completed-{Guid.NewGuid():N}",
            commenceTime.AddDays(-3),
            "Liverpool",
            "Everton",
            2,
            1);

        environment.OddsApi.SetUpcomingEvents(IntegrationEnvironment.League, IntegrationEnvironment.ApiKey,
            [upcomingEvent]);
        environment.OddsApi.SetCompletedEvents(IntegrationEnvironment.League, IntegrationEnvironment.ApiKey,
            [completedEvent]);

        var host = environment.FunctionsHost;

        // Act: the timer functions run on demand; the rest is triggered by the messages and documents they produce.
        await host.InvokeAsync("UpcomingEventsFunction", cancellationToken);
        await host.InvokeAsync("EventResultsFunction", cancellationToken);

        var published = await WaitForAsync(environment, async token =>
        {
            using var response = await host.Client.GetAsync("api/PredictionsHttpFunction", token);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var predictions = await response.Content.ReadFromJsonAsync<EventPrediction[]>(token);

            return predictions?.SingleOrDefault(p => p.Id == upcomingEvent.Id);
        }, "PredictionsHttpFunction to return the prediction", cancellationToken);

        var storedPrediction = await WaitForAsync(environment,
            token => environment.CosmosDb.TryReadItemAsync<EventPrediction>(IntegrationEnvironment.Database,
                IntegrationEnvironment.EventPredictionsContainer, upcomingEvent.Id, token),
            "the prediction in Cosmos DB", cancellationToken);

        var storedResult = await WaitForAsync(environment,
            token => environment.CosmosDb.TryReadItemAsync<EventResult>(IntegrationEnvironment.Database,
                IntegrationEnvironment.EventResultsContainer, completedEvent.Id, token),
            "the event result in Cosmos DB", cancellationToken);

        // Assert
        var expectedPrediction = new EventPrediction
        {
            Id = upcomingEvent.Id,
            HomeTeam = upcomingEvent.HomeTeam,
            AwayTeam = upcomingEvent.AwayTeam,
            CommenceTime = upcomingEvent.CommenceTime,
            Outcome = "HomeTeam"
        };

        using var scope = new AssertionScope();

        published.Should().BeEquivalentTo(expectedPrediction);
        storedPrediction.Should().BeEquivalentTo(expectedPrediction);
        storedResult.Should().BeEquivalentTo(new EventResult
        {
            Id = completedEvent.Id,
            CommenceTime = completedEvent.CommenceTime,
            Outcome = completedEvent.HomeTeam
        });
    }

    private static async Task<T> WaitForAsync<T>(IntegrationEnvironment environment,
        Func<CancellationToken, Task<T?>> probe, string description, CancellationToken cancellationToken)
        where T : class
    {
        try
        {
            return await Eventually.GetAsync(probe, Timeout, description, cancellationToken);
        }
        catch (TimeoutException exception)
        {
            throw new TimeoutException(
                $"{exception.Message}{Environment.NewLine}Functions host log:{Environment.NewLine}" +
                environment.FunctionsHost.Logs, exception.InnerException);
        }
    }
}
