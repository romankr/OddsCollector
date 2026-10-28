using System.Net;
using System.Text.Json;
using Microsoft.Azure.Cosmos;

namespace OddsCollector.Functions.IntegrationTests.Infrastructure.CosmosDb;

internal sealed class CosmosDbStore(string connectionString) : IDisposable
{
    private readonly CosmosClient _client = new(connectionString,
        new CosmosClientOptions
        {
            ConnectionMode = ConnectionMode.Gateway,
            LimitToEndpoint = true,
            UseSystemTextJsonSerializerWithOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        });

    public void Dispose()
    {
        _client.Dispose();
    }

    public async Task UpsertItemAsync<T>(string databaseName, string containerName, string id, T item,
        CancellationToken cancellationToken)
    {
        await _client.GetContainer(databaseName, containerName)
            .UpsertItemAsync(item, new PartitionKey(id), cancellationToken: cancellationToken);
    }

    public async Task DeleteItemAsync(string databaseName, string containerName, string id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _client.GetContainer(databaseName, containerName)
                .DeleteItemAsync<object>(id, new PartitionKey(id), cancellationToken: cancellationToken);
        }
        catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
        }
    }

    public async Task<T?> TryReadItemAsync<T>(string databaseName, string containerName, string id,
        CancellationToken cancellationToken) where T : class
    {
        try
        {
            var response = await _client.GetContainer(databaseName, containerName)
                .ReadItemAsync<T>(id, new PartitionKey(id), cancellationToken: cancellationToken);

            return response.Resource;
        }
        catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
