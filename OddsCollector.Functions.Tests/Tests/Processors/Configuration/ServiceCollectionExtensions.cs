using Microsoft.Extensions.DependencyInjection;
using OddsCollector.Functions.Processors.Configuration;
using FunctionApp = OddsCollector.Functions.Processors;

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
                x => x.ServiceType == typeof(FunctionApp.IEventResultProcessor)
                     && x.ImplementationType == typeof(FunctionApp.EventResultProcessor)
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
                x => x.ServiceType == typeof(FunctionApp.IUpcomingEventsProcessor)
                     && x.ImplementationType == typeof(FunctionApp.UpcomingEventsProcessor)
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
                x => x.ServiceType == typeof(FunctionApp.IPredictionProcessor)
                     && x.ImplementationType == typeof(FunctionApp.PredictionProcessor)
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
                x => x.ServiceType == typeof(FunctionApp.IPredictionHttpRequestProcessor)
                     && x.ImplementationType == typeof(FunctionApp.PredictionHttpRequestProcessor)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }
}
