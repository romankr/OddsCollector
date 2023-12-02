using Microsoft.Azure.Cosmos;

namespace OddsCollector.Functions.CosmosDb;

internal static class ContainerFactory
{
    public static Container CreateContainer(string? connectionString, string? databaseId, string? containerId)
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionString);
        ArgumentException.ThrowIfNullOrEmpty(databaseId);
        ArgumentException.ThrowIfNullOrEmpty(containerId);

        return new CosmosClient(connectionString).GetContainer(databaseId, containerId);
    }
}
