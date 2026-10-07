using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Azure.Cosmos;
using OddsCollector.Functions.IntegrationTests.Infrastructure.Network;

namespace OddsCollector.Functions.IntegrationTests.Infrastructure.CosmosDb;

/// <summary>
///     The Linux (vNext) Azure Cosmos DB emulator in a container.
/// </summary>
/// <remarks>
///     The emulator tells SDKs to reach it at its own port, so the container listens on the same port
///     as the host instead of a random mapping. It serves HTTPS with a self-signed certificate.
/// </remarks>
internal sealed class CosmosDbEmulator : IAsyncDisposable
{
    private const string DefaultImage = "mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-latest";
    private const int HealthPort = 8080;

    // The well-known key every Cosmos DB emulator accepts.
    private const string AccountKey =
        "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyZLKCSGCrkBf0QnnA==";

    private readonly IContainer _container;
    private readonly int _port;
    private CosmosClient? _client;

    public CosmosDbEmulator()
    {
        _port = FreePort.Get(HealthPort);

        _container = new ContainerBuilder(
                Environment.GetEnvironmentVariable("ODDSCOLLECTOR_COSMOSDB_EMULATOR_IMAGE") ?? DefaultImage)
            .WithEnvironment("PROTOCOL", "https")
            .WithEnvironment("PORT", _port.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .WithEnvironment("ENABLE_EXPLORER", "false")
            .WithEnvironment("ENABLE_TELEMETRY", "false")
            .WithPortBinding(_port, _port)
            .WithPortBinding(HealthPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(request => request.ForPort(HealthPort).ForPath("/ready")))
            .Build();
    }

    public string ConnectionString =>
        $"AccountEndpoint=https://localhost:{_port}/;AccountKey={AccountKey};DisableServerCertificateValidation=True;";

    public CosmosClient Client => _client ?? throw new InvalidOperationException("The emulator is not started");

    public async ValueTask DisposeAsync()
    {
        _client?.Dispose();
        await _container.DisposeAsync();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _container.StartAsync(cancellationToken);

        _client = new CosmosClient(ConnectionString,
            new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Gateway,
                LimitToEndpoint = true,
                // The models name their properties for System.Text.Json (for example "id"), as the
                // function app stores them.
                UseSystemTextJsonSerializerWithOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            });
    }

    public async Task CreateContainersAsync(string databaseName, IEnumerable<string> containerNames,
        CancellationToken cancellationToken)
    {
        Database database = await Client.CreateDatabaseIfNotExistsAsync(databaseName,
            cancellationToken: cancellationToken);

        foreach (var containerName in containerNames)
        {
            await database.CreateContainerIfNotExistsAsync(containerName, "/id",
                cancellationToken: cancellationToken);
        }
    }

    public async Task UpsertItemAsync<T>(string databaseName, string containerName, string id, T item,
        CancellationToken cancellationToken)
    {
        await Client.GetContainer(databaseName, containerName)
            .UpsertItemAsync(item, new PartitionKey(id), cancellationToken: cancellationToken);
    }

    /// <summary>
    ///     Deletes an item. An item that does not exist counts as deleted.
    /// </summary>
    public async Task DeleteItemAsync(string databaseName, string containerName, string id,
        CancellationToken cancellationToken)
    {
        try
        {
            await Client.GetContainer(databaseName, containerName)
                .DeleteItemAsync<object>(id, new PartitionKey(id), cancellationToken: cancellationToken);
        }
        catch (CosmosException exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Already gone.
        }
    }

    public async Task<T?> TryReadItemAsync<T>(string databaseName, string containerName, string id,
        CancellationToken cancellationToken) where T : class
    {
        try
        {
            var response = await Client.GetContainer(databaseName, containerName)
                .ReadItemAsync<T>(id, new PartitionKey(id), cancellationToken: cancellationToken);

            return response.Resource;
        }
        catch (CosmosException exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
