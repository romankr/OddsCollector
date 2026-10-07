using FluentAssertions.Execution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;
using FunctionApp = OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Configuration;

internal sealed class ServiceCollectionExtensions
{
    private static ServiceProvider BuildProvider(string? leagues = "league1;league2", string? apiKey = "key",
        string? baseUrl = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OddsApiClient:Leagues"] = leagues,
                ["OddsApiClient:ApiKey"] = apiKey,
                ["OddsApiClient:BaseUrl"] = baseUrl
            })
            .Build();

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddOddsApiClientWithDependencies(configuration);

        return services.BuildServiceProvider();
    }

    [Test]
    public void AddOddsApiClientWithDependencies_WithAllSettings_PassesStartupValidation()
    {
        using var provider = BuildProvider();

        var action = () => provider.GetRequiredService<IStartupValidator>().Validate();

        action.Should().NotThrow();
    }

    [TestCase(null, "key", "OddsApiClient:Leagues",
        TestName = "AddOddsApiClientWithDependencies_WithoutLeagues_FailsStartupValidation")]
    [TestCase("league1", null, "OddsApiClient:ApiKey",
        TestName = "AddOddsApiClientWithDependencies_WithoutApiKey_FailsStartupValidation")]
    public void AddOddsApiClientWithDependencies_WithMissingSetting_FailsStartupValidation(string? leagues,
        string? apiKey, string setting)
    {
        using var provider = BuildProvider(leagues, apiKey);

        var action = () => provider.GetRequiredService<IStartupValidator>().Validate();

        action.Should().Throw<OptionsValidationException>()
            .Which.Failures.Should().ContainSingle().Which.Should().StartWith(setting);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_WithEverySettingInvalid_ReportsEveryFailureAtOnce()
    {
        using var provider = BuildProvider(leagues: null, apiKey: null, baseUrl: "not a url");

        var action = () => provider.GetRequiredService<IStartupValidator>().Validate();

        action.Should().Throw<OptionsValidationException>().Which.Failures.Should().HaveCount(3);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_WithLeaguesAndApiKey_BindsThemToOptions()
    {
        using var provider = BuildProvider(leagues: "league1;league2", apiKey: "key");

        var options = provider.GetRequiredService<IOptions<FunctionApp.Configuration.OddsApiClientOptions>>().Value;

        using var scope = new AssertionScope();

        options.Leagues.Should().BeEquivalentTo("league1", "league2");
        options.ApiKey.Should().Be("key");
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingHttpClient_ReturnsTransientInstance()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<HttpClient>();

        provider.GetRequiredService<HttpClient>().Should().NotBeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingQuotaLoggingHandler_ReturnsTransientInstance()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<FunctionApp.QuotaLoggingHandler>();

        provider.GetRequiredService<FunctionApp.QuotaLoggingHandler>().Should().NotBeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingClient_ReturnsTransientInstance()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IClient>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<Client>();
        provider.GetRequiredService<IClient>().Should().NotBeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_WithoutBaseUrl_ResolvesClientForOddsApi()
    {
        using var provider = BuildProvider();

        var client = (Client)provider.GetRequiredService<IClient>();

        client.BaseUrl.Should().Be("https://api.the-odds-api.com/");
    }

    [Test]
    public void AddOddsApiClientWithDependencies_WithBaseUrl_ResolvesClientForBaseUrl()
    {
        using var provider = BuildProvider(baseUrl: "http://localhost:8080");

        var client = (Client)provider.GetRequiredService<IClient>();

        client.BaseUrl.Should().Be("http://localhost:8080/");
    }

    [Test]
    public void AddOddsApiClientWithDependencies_WithInvalidBaseUrl_FailsStartupValidation()
    {
        using var provider = BuildProvider(baseUrl: "not a url");

        var action = () => provider.GetRequiredService<IStartupValidator>().Validate();

        action.Should().Throw<OptionsValidationException>()
            .Which.Failures.Should().ContainSingle().Which.Should().StartWith("OddsApiClient:BaseUrl");
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingUpcomingEventsClient_ReturnsTransientInstance()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<FunctionApp.IUpcomingEventsClient>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<FunctionApp.UpcomingEventsClient>();
        provider.GetRequiredService<FunctionApp.IUpcomingEventsClient>().Should().NotBeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingEventResultsClient_ReturnsTransientInstance()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<FunctionApp.IEventResultsClient>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<FunctionApp.EventResultsClient>();
        provider.GetRequiredService<FunctionApp.IEventResultsClient>().Should().NotBeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingOriginalUpcomingEventConverter_ReturnsSingletonInstance()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IOriginalUpcomingEventConverter>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<OriginalUpcomingEventConverter>();
        provider.GetRequiredService<IOriginalUpcomingEventConverter>().Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingBookmakerConverter_ReturnsSingletonInstance()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IBookmakerConverter>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<BookmakerConverter>();
        provider.GetRequiredService<IBookmakerConverter>().Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingMarketConverter_ReturnsSingletonInstance()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IMarketConverter>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<MarketConverter>();
        provider.GetRequiredService<IMarketConverter>().Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingOddConverter_ReturnsSingletonInstance()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IOddConverter>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<OddConverter>();
        provider.GetRequiredService<IOddConverter>().Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingOriginalCompletedEventConverter_ReturnsSingletonInstance()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IOriginalCompletedEventConverter>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<OriginalCompletedEventConverter>();
        provider.GetRequiredService<IOriginalCompletedEventConverter>().Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingOutcomeConverter_ReturnsSingletonInstance()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IOutcomeConverter>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<OutcomeConverter>();
        provider.GetRequiredService<IOutcomeConverter>().Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingScoreModelsConverter_ReturnsSingletonInstance()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IScoreModelsConverter>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<ScoreModelsConverter>();
        provider.GetRequiredService<IScoreModelsConverter>().Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingScoreModelConverter_ReturnsSingletonInstance()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IScoreModelConverter>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<ScoreModelConverter>();
        provider.GetRequiredService<IScoreModelConverter>().Should().BeSameAs(first);
    }
}
