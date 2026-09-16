using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;
using FunctionApp = OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Configuration;

internal sealed class ServiceCollectionExtensions
{
    // Resolving the API first builds its first pipeline, and the handlers in it need a
    // logger, as do the two league clients.
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddOddsApiClientWithDependencies("league1;league2", "key");

        return services.BuildServiceProvider();
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvesOddsApiClientOptions()
    {
        using var provider = BuildProvider();

        var options = provider.GetRequiredService<IOptions<FunctionApp.Configuration.OddsApiClientOptions>>().Value;

        using var scope = new AssertionScope();

        options.Leagues.Should().BeEquivalentTo("league1", "league2");
        options.ApiKey.Should().Be("key");
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvesHttpClient()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<HttpClient>();

        using var scope = new AssertionScope();

        first.Should().NotBeNull();
        provider.GetRequiredService<HttpClient>().Should().NotBeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvesQuotaLoggingHandler()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<FunctionApp.QuotaLoggingHandler>();

        using var scope = new AssertionScope();

        first.Should().NotBeNull();
        provider.GetRequiredService<FunctionApp.QuotaLoggingHandler>().Should().NotBeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvesClient()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IClient>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<Client>();
        provider.GetRequiredService<IClient>().Should().NotBeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvesUpcomingEventsClient()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<FunctionApp.IUpcomingEventsClient>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<FunctionApp.UpcomingEventsClient>();
        provider.GetRequiredService<FunctionApp.IUpcomingEventsClient>().Should().NotBeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvesEventResultsClient()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<FunctionApp.IEventResultsClient>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<FunctionApp.EventResultsClient>();
        provider.GetRequiredService<FunctionApp.IEventResultsClient>().Should().NotBeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvesOriginalUpcomingEventConverter()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IOriginalUpcomingEventConverter>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<OriginalUpcomingEventConverter>();
        provider.GetRequiredService<IOriginalUpcomingEventConverter>().Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvesBookmakerConverter()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IBookmakerConverter>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<BookmakerConverter>();
        provider.GetRequiredService<IBookmakerConverter>().Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvesMarketConverter()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IMarketConverter>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<MarketConverter>();
        provider.GetRequiredService<IMarketConverter>().Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvesOutcomeConverter()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IOutcomeConverter>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<OutcomeConverter>();
        provider.GetRequiredService<IOutcomeConverter>().Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvesOriginalCompletedEventConverter()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IOriginalCompletedEventConverter>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<OriginalCompletedEventConverter>();
        provider.GetRequiredService<IOriginalCompletedEventConverter>().Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvesWinnerConverter()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IWinnerConverter>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<WinnerConverter>();
        provider.GetRequiredService<IWinnerConverter>().Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvesScoreModelsConverter()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IScoreModelsConverter>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<ScoreModelsConverter>();
        provider.GetRequiredService<IScoreModelsConverter>().Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvesScoreModelConverter()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IScoreModelConverter>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<ScoreModelConverter>();
        provider.GetRequiredService<IScoreModelConverter>().Should().BeSameAs(first);
    }
}
