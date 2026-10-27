using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Azure.Messaging.ServiceBus;
using OddsCollector.AppHost;
using OddsCollector.Functions.IntegrationTests.Infrastructure.CosmosDb;
using OddsCollector.Functions.IntegrationTests.Infrastructure.Functions;
using OddsCollector.Functions.IntegrationTests.Infrastructure.OddsApi;
using OddsCollector.Functions.IntegrationTests.Infrastructure.Polling;
using OddsCollector.Functions.IntegrationTests.Infrastructure.ServiceBus;
using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.IntegrationTests.Infrastructure;

// The function app and its emulators (Cosmos DB, Service Bus, Azurite), started by OddsCollector.AppHost through
// Aspire, with a stub of The Odds API in place of the real one.
internal sealed class IntegrationEnvironment : IAsyncDisposable
{
    public const string ApiKey = "integration-test-key";
    public const string League = "soccer_epl";
    public const string Database = AppHostSettings.DatabaseName;
    public const string EventPredictionsContainer = AppHostSettings.EventPredictionsContainer;
    public const string EventResultsContainer = AppHostSettings.EventResultsContainer;

    private const string NeverDuringTests = "0 0 0 1 1 *";

    private static readonly TimeSpan WaitTimeout = TimeSpan.FromMinutes(2);

    private DistributedApplication? _app;
    private CosmosDbStore? _cosmosDb;
    private FunctionsHost? _functionsHost;
    private ServiceBusQueue? _serviceBusQueue;

    private IntegrationEnvironment()
    {
    }

    public OddsApiStub OddsApi { get; } = new(League, ApiKey);

    public FunctionsHost FunctionsHost =>
        _functionsHost ?? throw new InvalidOperationException("The environment is not started");

    public ServiceBusQueue ServiceBusQueue =>
        _serviceBusQueue ?? throw new InvalidOperationException("The environment is not started");

    private CosmosDbStore CosmosDb =>
        _cosmosDb ?? throw new InvalidOperationException("The environment is not started");

    public async ValueTask DisposeAsync()
    {
        if (_functionsHost is not null)
        {
            await _functionsHost.DisposeAsync();
        }

        if (_serviceBusQueue is not null)
        {
            await _serviceBusQueue.DisposeAsync();
        }

        _cosmosDb?.Dispose();

        if (_app is not null)
        {
            await _app.DisposeAsync();
        }

        OddsApi.Dispose();
    }

    public static async Task<IntegrationEnvironment> StartAsync(CancellationToken cancellationToken)
    {
        var environment = new IntegrationEnvironment();

        try
        {
            await environment.StartAppAsync(cancellationToken);
        }
        catch
        {
            await environment.DisposeAsync();
            throw;
        }

        return environment;
    }

    public Task StorePredictionAsync(EventPrediction prediction, CancellationToken cancellationToken)
    {
        return CosmosDb.UpsertItemAsync(Database, EventPredictionsContainer, prediction.Id, prediction,
            cancellationToken);
    }

    public Task StorePredictionDocumentAsync<T>(string id, T document, CancellationToken cancellationToken)
    {
        return CosmosDb.UpsertItemAsync(Database, EventPredictionsContainer, id, document, cancellationToken);
    }

    public Task DeletePredictionAsync(string id, CancellationToken cancellationToken)
    {
        return CosmosDb.DeleteItemAsync(Database, EventPredictionsContainer, id, cancellationToken);
    }

    public Task<EventPrediction> WaitForStoredPredictionAsync(string id, CancellationToken cancellationToken)
    {
        return WaitForAsync(
            token => CosmosDb.TryReadItemAsync<EventPrediction>(Database, EventPredictionsContainer, id, token),
            $"prediction {id} in Cosmos DB", cancellationToken);
    }

    public Task<EventPrediction?> TryReadPredictionAsync(string id, CancellationToken cancellationToken)
    {
        return CosmosDb.TryReadItemAsync<EventPrediction>(Database, EventPredictionsContainer, id, cancellationToken);
    }

    public Task<EventResult> WaitForStoredResultAsync(string id, CancellationToken cancellationToken)
    {
        return WaitForAsync(
            token => CosmosDb.TryReadItemAsync<EventResult>(Database, EventResultsContainer, id, token),
            $"event result {id} in Cosmos DB", cancellationToken);
    }

    public Task<EventPrediction> WaitForPublishedPredictionAsync(string id, CancellationToken cancellationToken)
    {
        return WaitForAsync(
            async token => (await FunctionsHost.TryGetPredictionsAsync(token))?.SingleOrDefault(p => p.Id == id),
            $"PredictionsHttpFunction to return prediction {id}", cancellationToken);
    }

    public Task<EventPrediction[]> WaitForPublishedPredictionsAsync(string containingId,
        CancellationToken cancellationToken)
    {
        return WaitForAsync(
            async token =>
            {
                var predictions = await FunctionsHost.TryGetPredictionsAsync(token);

                return predictions?.Any(p => p.Id == containingId) == true ? predictions : null;
            },
            $"PredictionsHttpFunction to return prediction {containingId}", cancellationToken);
    }

    public Task<ServiceBusReceivedMessage> WaitForDeadLetteredMessageAsync(string messageId,
        CancellationToken cancellationToken)
    {
        return WaitForAsync(token => ServiceBusQueue.TryPeekDeadLetteredAsync(messageId, token),
            $"message {messageId} in the dead-letter queue", cancellationToken);
    }

    public Task<string> WaitForLogLineAsync(long afterLoggedLines, string text, CancellationToken cancellationToken)
    {
        return WaitForAsync(
            token => Task.FromResult(FunctionsHost.GetLogLinesAfter(afterLoggedLines)
                .FirstOrDefault(l => l.Contains(text, StringComparison.Ordinal))),
            $"the Functions host to log \"{text}\"", cancellationToken);
    }

    private async Task<T> WaitForAsync<T>(Func<CancellationToken, Task<T?>> probe, string description,
        CancellationToken cancellationToken) where T : class
    {
        try
        {
            return await Eventually.GetAsync(probe, WaitTimeout, description, cancellationToken);
        }
        catch (TimeoutException exception)
        {
            throw new TimeoutException(
                $"{exception.Message}{Environment.NewLine}Functions host log:{Environment.NewLine}" +
                FunctionsHost.Logs, exception.InnerException);
        }
    }

    private async Task StartAppAsync(CancellationToken cancellationToken)
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.OddsCollector_AppHost>(
            GetAppHostArguments(), cancellationToken);

        _app = await appHost.BuildAsync(cancellationToken);

        await _app.StartAsync(cancellationToken);

        _cosmosDb = new CosmosDbStore(
            await GetConnectionStringAsync(_app, AppHostSettings.CosmosResource, cancellationToken));

        _serviceBusQueue = new ServiceBusQueue(
            await GetConnectionStringAsync(_app, AppHostSettings.ServiceBusResource, cancellationToken),
            AppHostSettings.Queue);

        _functionsHost = await FunctionsHost.StartAsync(_app, cancellationToken);
    }

    // Settings the AppHost reads from its configuration (see OddsCollector.AppHost/Program.cs).
    private string[] GetAppHostArguments()
    {
        return
        [
            $"Parameters:{AppHostSettings.OddsApiKeyParameter}={ApiKey}",
            $"{AppHostSettings.Section}:Leagues={League}",
            $"{AppHostSettings.Section}:OddsApiBaseUrl={OddsApi.BaseUrl}",
            $"{AppHostSettings.Section}:UpcomingEventsTimerInterval={NeverDuringTests}",
            $"{AppHostSettings.Section}:EventResultsTimerInterval={NeverDuringTests}"
        ];
    }

    private static async Task<string> GetConnectionStringAsync(DistributedApplication app, string resourceName,
        CancellationToken cancellationToken)
    {
        return await app.GetConnectionStringAsync(resourceName, cancellationToken)
               ?? throw new InvalidOperationException($"The AppHost has no connection string for {resourceName}");
    }
}
