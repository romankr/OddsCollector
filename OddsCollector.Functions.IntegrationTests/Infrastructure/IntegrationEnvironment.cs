using OddsCollector.Functions.IntegrationTests.Infrastructure.CosmosDb;
using OddsCollector.Functions.IntegrationTests.Infrastructure.Functions;
using OddsCollector.Functions.IntegrationTests.Infrastructure.OddsApi;
using OddsCollector.Functions.IntegrationTests.Infrastructure.Polling;
using OddsCollector.Functions.Models;
using Testcontainers.Azurite;
using Testcontainers.ServiceBus;

namespace OddsCollector.Functions.IntegrationTests.Infrastructure;

/// <summary>
///     A local Azure environment for the function app: Azurite for the host storage, the Service Bus
///     emulator, the Cosmos DB emulator, a stub of The Odds API and the Azure Functions host itself.
/// </summary>
/// <remarks>Needs Docker and Azure Functions Core Tools (func).</remarks>
internal sealed class IntegrationEnvironment : IAsyncDisposable
{
    public const string ApiKey = "integration-test-key";
    public const string League = "soccer_epl";
    public const string Database = "OddsCollector";
    public const string EventPredictionsContainer = "EventPredictions";
    public const string EventResultsContainer = "EventResults";

    // Must match the queue in Infrastructure/ServiceBus/Config.json.
    public const string Queue = "upcoming-events";

    // Once a year: timers never fire on their own during a test run, the tests invoke them instead.
    private const string NeverDuringTests = "0 0 0 1 1 *";

    private const string AzuriteImage = "mcr.microsoft.com/azure-storage/azurite:latest";
    private const string ServiceBusImage = "mcr.microsoft.com/azure-messaging/servicebus-emulator:latest";

    // The exporter needs a connection string to start; nothing listens on this endpoint.
    private const string ApplicationInsightsConnectionString =
        "InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=http://127.0.0.1:9/";

    // How long the function app gets to produce a result after a test triggers it.
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromMinutes(2);

    private readonly AzuriteContainer _azurite = new AzuriteBuilder(AzuriteImage).Build();

    private readonly ServiceBusContainer _serviceBus = new ServiceBusBuilder(ServiceBusImage)
        .WithAcceptLicenseAgreement(true)
        .WithConfig(Path.Combine(AppContext.BaseDirectory, "Infrastructure", "ServiceBus", "Config.json"))
        .Build();

    private FunctionsHost? _functionsHost;

    private IntegrationEnvironment()
    {
    }

    public CosmosDbEmulator CosmosDb { get; } = new();

    public OddsApiStub OddsApi { get; } = new(League, ApiKey);

    public FunctionsHost FunctionsHost =>
        _functionsHost ?? throw new InvalidOperationException("The environment is not started");

    public async ValueTask DisposeAsync()
    {
        if (_functionsHost is not null)
        {
            await _functionsHost.DisposeAsync();
        }

        OddsApi.Dispose();

        await Task.WhenAll(
            _azurite.DisposeAsync().AsTask(),
            _serviceBus.DisposeAsync().AsTask(),
            CosmosDb.DisposeAsync().AsTask());
    }

    /// <summary>
    ///     Starts the environment. The caller owns it and disposes it, which stops every part of it.
    /// </summary>
    public static async Task<IntegrationEnvironment> StartAsync(CancellationToken cancellationToken)
    {
        var environment = new IntegrationEnvironment();

        try
        {
            await environment.StartPartsAsync(cancellationToken);
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

    public Task<EventPrediction> WaitForStoredPredictionAsync(string id, CancellationToken cancellationToken)
    {
        return WaitForAsync(
            token => CosmosDb.TryReadItemAsync<EventPrediction>(Database, EventPredictionsContainer, id, token),
            $"prediction {id} in Cosmos DB", cancellationToken);
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

    /// <summary>
    ///     Waits for a side effect of the function app and adds the host log to the error if it never comes.
    /// </summary>
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

    private async Task StartPartsAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            _azurite.StartAsync(cancellationToken),
            _serviceBus.StartAsync(cancellationToken),
            CosmosDb.StartAsync(cancellationToken));

        // The output bindings write to existing containers only, as in Azure.
        await CosmosDb.CreateContainersAsync(Database, [EventPredictionsContainer, EventResultsContainer],
            cancellationToken);

        _functionsHost = await FunctionsHost.StartAsync(GetAppSettings(), cancellationToken);
    }

    private Dictionary<string, string> GetAppSettings()
    {
        return new Dictionary<string, string>
        {
            ["FUNCTIONS_WORKER_RUNTIME"] = "dotnet-isolated",
            ["AzureWebJobsStorage"] = _azurite.GetConnectionString(),
            ["APPLICATIONINSIGHTS_CONNECTION_STRING"] = ApplicationInsightsConnectionString,
            ["OddsApiClient__ApiKey"] = ApiKey,
            ["OddsApiClient__Leagues"] = League,
            ["OddsApiClient__BaseUrl"] = OddsApi.BaseUrl,
            ["CosmosDb__Connection"] = CosmosDb.ConnectionString,
            ["CosmosDb__Database"] = Database,
            ["CosmosDb__EventPredictionsContainer"] = EventPredictionsContainer,
            ["CosmosDb__EventResultsContainer"] = EventResultsContainer,
            ["ServiceBus__Connection"] = _serviceBus.GetConnectionString(),
            ["ServiceBus__Queue"] = Queue,
            ["EventResultsFunction__TimerInterval"] = NeverDuringTests,
            ["UpcomingEventsFunction__TimerInterval"] = NeverDuringTests,
            // The Cosmos DB emulator supports gateway mode only.
            ["AzureFunctionsJobHost__extensions__cosmosDB__connectionMode"] = "Gateway"
        };
    }
}
