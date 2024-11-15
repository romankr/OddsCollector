using Microsoft.Extensions.DependencyInjection;
using OddsCollector.Functions.Processors;
using OddsCollector.Functions.Processors.Configuration;

namespace OddsCollector.Functions.Tests.Tests.Processors;

internal class ServiceCollectionExtensions
{
    [Test]
    public void AddFunctionProcessors_AddsEventResultProcessor()
    {
        var services = new ServiceCollection();

        services.AddFunctionProcessors();

        var descriptor =
            services.FirstOrDefault(
                x => x.ServiceType == typeof(IEventResultProcessor)
                     && x.ImplementationType == typeof(OddsCollector.Functions.Processors.EventResultProcessor)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }

    [Test]
    public void AddFunctionProcessors_AddsUpcomingEventsProcessor()
    {
        var services = new ServiceCollection();

        services.AddFunctionProcessors();

        var descriptor =
            services.FirstOrDefault(
                x => x.ServiceType == typeof(IUpcomingEventsProcessor)
                     && x.ImplementationType == typeof(OddsCollector.Functions.Processors.UpcomingEventsProcessor)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }
}
