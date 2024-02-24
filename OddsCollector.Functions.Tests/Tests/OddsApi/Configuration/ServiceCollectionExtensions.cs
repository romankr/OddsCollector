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
        services.AddOddsApiClientWithDependencies();

        // Assert
        var options =
            services.FirstOrDefault(
                x => x.ServiceType == typeof(IConfigureOptions<OddsApiClientOptions>)
                     && x.Lifetime == ServiceLifetime.Singleton);

        options.Should().NotBeNull();

        var httpClient =
            services.FirstOrDefault(
                x => x.ServiceType == typeof(HttpClient)
                     && x.Lifetime == ServiceLifetime.Transient);

        httpClient.Should().NotBeNull();

        var client =
            services.FirstOrDefault(
                x => x.ImplementationType == typeof(Client)
                     && x.ServiceType == typeof(IClient)
                     && x.Lifetime == ServiceLifetime.Singleton);

        client.Should().NotBeNull();

        var oddsApiClient =
            services.FirstOrDefault(
                x => x.ImplementationType == typeof(OddsCollector.Functions.OddsApi.OddsApiClient)
                     && x.ServiceType == typeof(IOddsApiClient)
                     && x.Lifetime == ServiceLifetime.Singleton);

        oddsApiClient.Should().NotBeNull();
    }
}
