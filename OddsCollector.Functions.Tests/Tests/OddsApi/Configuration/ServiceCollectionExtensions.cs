using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OddsCollector.Functions.OddsApi;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Configuration;

internal class ServiceCollectionExtensions
{
    [Test]
    public void AddOddsApiClientWithDependencies_AddsProperlyConfiguredOddsApiClient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOddsApiClientWithDependencies("leagues", "key");

        // Assert
        var optionsDescriptor =
            services.FirstOrDefault(
                x => x.ServiceType == typeof(IConfigureOptions<OddsApiClientOptions>)
                     && x.Lifetime == ServiceLifetime.Singleton);

        optionsDescriptor.Should().NotBeNull();

        var httpClientDescriptor =
            services.FirstOrDefault(
                x => x.ServiceType == typeof(HttpClient)
                     && x.Lifetime == ServiceLifetime.Transient);

        httpClientDescriptor.Should().NotBeNull();

        var clientDescriptor =
            services.FirstOrDefault(
                x => x.ImplementationType == typeof(Client)
                     && x.ServiceType == typeof(IClient)
                     && x.Lifetime == ServiceLifetime.Singleton);

        clientDescriptor.Should().NotBeNull();

        var oddsApiClientDescriptor =
            services.FirstOrDefault(
                x => x.ImplementationType == typeof(OddsCollector.Functions.OddsApi.OddsApiClient)
                     && x.ServiceType == typeof(IOddsApiClient)
                     && x.Lifetime == ServiceLifetime.Singleton);

        oddsApiClientDescriptor.Should().NotBeNull();
    }
}
