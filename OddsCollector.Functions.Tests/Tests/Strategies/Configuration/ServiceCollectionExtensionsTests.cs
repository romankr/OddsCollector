using Microsoft.Extensions.DependencyInjection;
using OddsCollector.Functions.Strategies;
using OddsCollector.Functions.Strategies.Configuration;

namespace OddsCollector.Functions.Tests.Tests.Strategies.Configuration;

[Parallelizable(ParallelScope.All)]
internal class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddPredictionStrategy_AddsProperlyConfiguredPredictionStrategy()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPredictionStrategy();

        // Assert
        var strategy =
            services.FirstOrDefault(
                x => x.ServiceType == typeof(IPredictionStrategy)
                     && x.ImplementationType == typeof(AdjustedConsensusStrategy)
                     && x.Lifetime == ServiceLifetime.Singleton);

        strategy.Should().NotBeNull();
    }
}
