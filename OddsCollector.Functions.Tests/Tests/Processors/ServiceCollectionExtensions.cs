using Microsoft.Extensions.DependencyInjection;
using OddsCollector.Functions.Processors;
using OddsCollector.Functions.Processors.Configuration;

namespace OddsCollector.Functions.Tests.Tests.Processors;

internal class ServiceCollectionExtensions
{
    [Test]
    public void AddFunctionProcessors_AddsProperlyConfiguredFunctionProcessors()
    {
        var services = new ServiceCollection();

        services.AddFunctionProcessors();

        var strategyDescriptor =
            services.FirstOrDefault(
                x => x.ServiceType == typeof(IEventResultProcessor)
                     && x.ImplementationType == typeof(OddsCollector.Functions.Processors.EventResultProcessor)
                     && x.Lifetime == ServiceLifetime.Singleton);

        strategyDescriptor.Should().NotBeNull();
    }
}
