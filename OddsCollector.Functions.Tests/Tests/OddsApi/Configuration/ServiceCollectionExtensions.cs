using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OddsCollector.Functions.OddsApi;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Configuration;

internal sealed class ServiceCollectionExtensions
{
    [Test]
    public void AddOddsApiClientWithDependencies_AddsOddsApiClientOptions()
    {
        var services = new ServiceCollection();

        services.AddOddsApiClientWithDependencies("leagues", "key");

        var descriptor =
            services.FirstOrDefault(
                x => x.ServiceType == typeof(IConfigureOptions<OddsApiClientOptions>)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }

    [Test]
    public void AddOddsApiClientWithDependencies_AddsHttpClient()
    {
        var services = new ServiceCollection();

        services.AddOddsApiClientWithDependencies("leagues", "key");

        var descriptor =
            services.FirstOrDefault(
                x => x.ServiceType == typeof(HttpClient)
                     && x.Lifetime == ServiceLifetime.Transient);

        descriptor.Should().NotBeNull();
    }

    [Test]
    public void AddOddsApiClientWithDependencies_AddsClient()
    {
        var services = new ServiceCollection();

        services.AddOddsApiClientWithDependencies("leagues", "key");

        var descriptor =
            services.FirstOrDefault(
                x => x.ImplementationType == typeof(Client)
                     && x.ServiceType == typeof(IClient)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }

    [Test]
    public void AddOddsApiClientWithDependencies_AddsOddsApiClient()
    {
        var services = new ServiceCollection();

        services.AddOddsApiClientWithDependencies("leagues", "key");

        var descriptor =
            services.FirstOrDefault(
                x => x.ImplementationType == typeof(OddsCollector.Functions.OddsApi.OddsApiClient)
                     && x.ServiceType == typeof(IOddsApiClient)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }
}
