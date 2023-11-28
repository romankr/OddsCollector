using FluentAssertions;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using OddsCollector.Functions.Notification.CosmosDb;

namespace OddsCollector.Functions.Notification.Tests.CosmosDb;

internal class CosmosDbClientTests
{
    [Test]
    public void Constructor_WithValidDependencies_ReturnsNewInstance()
    {
        var containerStub = Substitute.For<Container>();

        var result = new CosmosDbClient(containerStub);

        result.Should().NotBeNull();
    }

    [Test]
    public void Constructor_WithNullContainer_ThrowsException()
    {
        var action = () =>
        {
            _ = new CosmosDbClient(null);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("container");
    }

    // todo: test for GetEventPredictionsAsync() - hard to mock all the dependencies atm
}
