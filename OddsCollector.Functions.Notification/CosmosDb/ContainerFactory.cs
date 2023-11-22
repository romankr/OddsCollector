using Microsoft.Azure.Cosmos;

namespace OddsCollector.Functions.Notification.CosmosDb;

internal static class ContainerFactory
{
    public static Container CreateContainer(string? connectionString, string? databaseId, string? containerId)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException($"{nameof(connectionString)} cannot be null or empty",
                nameof(connectionString));
        }

        if (string.IsNullOrEmpty(databaseId))
        {
            throw new ArgumentException($"{nameof(databaseId)} cannot be null or empty", nameof(databaseId));
        }

        if (string.IsNullOrEmpty(containerId))
        {
            throw new ArgumentException($"{nameof(containerId)} cannot be null or empty", nameof(containerId));
        }

        return new CosmosClient(connectionString).GetContainer(databaseId, containerId);
    }
}
