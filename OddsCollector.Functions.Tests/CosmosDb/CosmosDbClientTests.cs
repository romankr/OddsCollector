using Microsoft.Azure.Cosmos;
using OddsCollector.Functions.CosmosDb;

namespace OddsCollector.Functions.Tests.CosmosDb;

[Parallelizable(ParallelScope.All)]
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
