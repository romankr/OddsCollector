using Microsoft.Extensions.DependencyInjection;
using OddsCollector.Functions.Strategies;
using OddsCollector.Functions.Strategies.Configuration;

namespace OddsCollector.Functions.Tests.Tests.Strategies.Configuration;

[Parallelizable(ParallelScope.All)]
internal class ServiceCollectionExtensions
{
    [Test]
    public void AddPredictionStrategy_AddsProperlyConfiguredPredictionStrategy()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPredictionStrategy();

        // Assert
        var strategyDescriptor =
            services.FirstOrDefault(
                x => x.ServiceType == typeof(IPredictionStrategy)
                     && x.ImplementationType == typeof(OddsCollector.Functions.Strategies.AdjustedConsensusStrategy)
                     && x.Lifetime == ServiceLifetime.Singleton);

        strategyDescriptor.Should().NotBeNull();
    }
}
