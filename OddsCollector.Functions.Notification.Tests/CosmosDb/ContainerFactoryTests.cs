using FluentAssertions;
using OddsCollector.Functions.Notification.CosmosDb;

namespace OddsCollector.Functions.Notification.Tests.CosmosDb;

[Parallelizable(ParallelScope.All)]
internal class ContainerFactoryTests
{
    [Test]
    public void CreateContainer_WithValidParameters_ReturnsNewInstance()
    {
        string databaseId = nameof(databaseId);

        var container = ContainerFactory.CreateContainer(
            "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=test", databaseId, "containerId");

        container.Should().NotBeNull();
        container.Database.Should().NotBeNull();
        container.Database.Id.Should().NotBeNull().And.Be(databaseId);
    }

    [TestCase("")]
    [TestCase(null)]
    public void CreateContainer_WithNullOrEmptyConnectionString_ThrowsException(string? connectionString)
    {
        var action = () =>
        {
            _ = ContainerFactory.CreateContainer(connectionString, "databaseId", "containerId");
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(connectionString));
    }

    [TestCase("")]
    [TestCase(null)]
    public void CreateContainer_WithNullOrEmptyDatabaseId_ThrowsException(string? databaseId)
    {
        var action = () =>
        {
            _ = ContainerFactory.CreateContainer(
                "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=test", databaseId, "containerId");
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(databaseId));
    }

    [TestCase("")]
    [TestCase(null)]
    public void CreateContainer_WithNullOrEmptyContainerId_ThrowsException(string? containerId)
    {
        var action = () =>
        {
            _ = ContainerFactory.CreateContainer(
                "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=test", "databaseId", containerId);
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(containerId));
    }
}
