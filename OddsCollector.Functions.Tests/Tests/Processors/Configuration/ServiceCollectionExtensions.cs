using Microsoft.Extensions.DependencyInjection;
using OddsCollector.Functions.Processors;
using OddsCollector.Functions.Processors.Configuration;
using FunctionsApp = OddsCollector.Functions.Processors;

namespace OddsCollector.Functions.Tests.Tests.Processors.Configuration;

internal sealed class ServiceCollectionExtensions
{
    [Test]
    public void AddFunctionProcessors_AddsEventResultProcessor()
    {
        var services = new ServiceCollection();

        services.AddFunctionProcessors();

        var descriptor =
            services.FirstOrDefault(
                x => x.ServiceType == typeof(IEventResultProcessor)
                     && x.ImplementationType == typeof(FunctionsApp.EventResultProcessor)
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
                     && x.ImplementationType == typeof(FunctionsApp.UpcomingEventsProcessor)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }

    [Test]
    public void AddFunctionProcessors_AddsPredictionProcessor()
    {
        var services = new ServiceCollection();

        services.AddFunctionProcessors();

        var descriptor =
            services.FirstOrDefault(
                x => x.ServiceType == typeof(IPredictionProcessor)
                     && x.ImplementationType == typeof(FunctionsApp.PredictionProcessor)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }

    [Test]
    public void AddFunctionProcessors_AddsPredictionHttpRequestProcessor()
    {
        var services = new ServiceCollection();

        services.AddFunctionProcessors();

        var descriptor =
            services.FirstOrDefault(
                x => x.ServiceType == typeof(IPredictionHttpRequestProcessor)
                     && x.ImplementationType == typeof(FunctionsApp.PredictionHttpRequestProcessor)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }
}
