﻿using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OddsCollector.Common.OddsApi;
using OddsCollector.Common.OddsApi.Configuration;
using OddsCollector.Common.OddsApi.WebApi;

namespace OddsCollector.Common.Tests.OddsApi.Configuration;

internal class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddOddsApiClientWithDependencies_AddsProperlyConfiguredOddsApiClient()
    {
        var services = new ServiceCollection();

        services.AddOddsApiClientWithDependencies();

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
                x => x.ImplementationType == typeof(OddsApiClient)
                && x.ServiceType == typeof(IOddsApiClient)
                && x.Lifetime == ServiceLifetime.Singleton);

        oddsApiClient.Should().NotBeNull();
    }
}
