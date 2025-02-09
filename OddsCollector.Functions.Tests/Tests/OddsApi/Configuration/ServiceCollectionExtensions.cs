using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OddsCollector.Functions.OddsApi;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;
using FunctionsApp = OddsCollector.Functions.OddsApi;

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
                x => x.ServiceType == typeof(IConfigureOptions<FunctionsApp.Configuration.OddsApiClientOptions>)
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
    public void AddOddsApiClientWithDependencies_AddsUpcomingEventsClient()
    {
        var services = new ServiceCollection();

        services.AddOddsApiClientWithDependencies("leagues", "key");

        var descriptor =
            services.FirstOrDefault(
                x => x.ImplementationType == typeof(FunctionsApp.UpcomingEventsClient)
                     && x.ServiceType == typeof(IUpcomingEventsClient)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }

    [Test]
    public void AddOddsApiClientWithDependencies_AddsOriginalUpcomingEventConverter()
    {
        var services = new ServiceCollection();

        services.AddOddsApiClientWithDependencies("leagues", "key");

        var descriptor =
            services.FirstOrDefault(
                x => x.ImplementationType == typeof(OriginalUpcomingEventConverter)
                     && x.ServiceType == typeof(IOriginalUpcomingEventConverter)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }

    [Test]
    public void AddOddsApiClientWithDependencies_AddsBookmakerConverter()
    {
        var services = new ServiceCollection();

        services.AddOddsApiClientWithDependencies("leagues", "key");

        var descriptor =
            services.FirstOrDefault(
                x => x.ImplementationType == typeof(BookmakerConverter)
                     && x.ServiceType == typeof(IBookmakerConverter)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }

    [Test]
    public void AddOddsApiClientWithDependencies_AddsMarketConverter()
    {
        var services = new ServiceCollection();

        services.AddOddsApiClientWithDependencies("leagues", "key");

        var descriptor =
            services.FirstOrDefault(
                x => x.ImplementationType == typeof(MarketConverter)
                     && x.ServiceType == typeof(IMarketConverter)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }

    [Test]
    public void AddOddsApiClientWithDependencies_AddsOutcomeConverter()
    {
        var services = new ServiceCollection();

        services.AddOddsApiClientWithDependencies("leagues", "key");

        var descriptor =
            services.FirstOrDefault(
                x => x.ImplementationType == typeof(OutcomeConverter)
                     && x.ServiceType == typeof(IOutcomeConverter)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }

    [Test]
    public void AddOddsApiClientWithDependencies_AddsEventResultsClient()
    {
        var services = new ServiceCollection();

        services.AddOddsApiClientWithDependencies("leagues", "key");

        var descriptor =
            services.FirstOrDefault(
                x => x.ImplementationType == typeof(FunctionsApp.EventResultsClient)
                     && x.ServiceType == typeof(IEventResultsClient)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }

    [Test]
    public void AddOddsApiClientWithDependencies_AddsOriginalCompletedEventConverter()
    {
        var services = new ServiceCollection();

        services.AddOddsApiClientWithDependencies("leagues", "key");

        var descriptor =
            services.FirstOrDefault(
                x => x.ImplementationType == typeof(OriginalCompletedEventConverter)
                     && x.ServiceType == typeof(IOriginalCompletedEventConverter)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }

    [Test]
    public void AddOddsApiClientWithDependencies_AddsWinnerConverter()
    {
        var services = new ServiceCollection();

        services.AddOddsApiClientWithDependencies("leagues", "key");

        var descriptor =
            services.FirstOrDefault(
                x => x.ImplementationType == typeof(WinnerConverter)
                     && x.ServiceType == typeof(IWinnerConverter)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }

    [Test]
    public void AddOddsApiClientWithDependencies_AddsScoreModelsConverter()
    {
        var services = new ServiceCollection();

        services.AddOddsApiClientWithDependencies("leagues", "key");

        var descriptor =
            services.FirstOrDefault(
                x => x.ImplementationType == typeof(ScoreModelsConverter)
                     && x.ServiceType == typeof(IScoreModelsConverter)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }

    [Test]
    public void AddOddsApiClientWithDependencies_AddsScoreModelConverter()
    {
        var services = new ServiceCollection();

        services.AddOddsApiClientWithDependencies("leagues", "key");

        var descriptor =
            services.FirstOrDefault(
                x => x.ImplementationType == typeof(ScoreModelConverter)
                     && x.ServiceType == typeof(IScoreModelConverter)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }
}
