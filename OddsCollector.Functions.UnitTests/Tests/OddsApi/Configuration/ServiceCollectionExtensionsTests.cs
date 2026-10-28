using System.Net;
using FluentAssertions.Execution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using OddsCollector.Functions.OddsApi;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;
using OddsCollector.Functions.UnitTests.Infrastructure.Http;

namespace OddsCollector.Functions.UnitTests.Tests.OddsApi.Configuration;

internal sealed class ServiceCollectionExtensionsTests
{
    private const string League = "soccer_epl";
    private const string ApiKey = "key";
    private const int DaysFrom = 3;

    private static ServiceProvider BuildProvider(string? leagues = "soccer_epl;soccer_spain_la_liga",
        string? apiKey = "key", string? baseUrl = null, HttpMessageHandler? primaryHandler = null)
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

        if (primaryHandler is not null)
        {
            services.AddHttpClient(nameof(IClient)).ConfigurePrimaryHttpMessageHandler(() => primaryHandler);

            services.Configure<HttpStandardResilienceOptions>($"{nameof(IClient)}-standard",
                options => options.Retry.Delay = TimeSpan.Zero);
        }

        return services.BuildServiceProvider();
    }

    [Test]
    public void AddOddsApiClientWithDependencies_WithAllSettings_PassesStartupValidation()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        var action = () => provider.GetRequiredService<IStartupValidator>().Validate();

        // Assert
        action.Should().NotThrow();
    }

    [TestCase(null, "key", "OddsApiClient:Leagues",
        TestName = "AddOddsApiClientWithDependencies_WithoutLeagues_FailsStartupValidation")]
    [TestCase("soccer_epl", null, "OddsApiClient:ApiKey",
        TestName = "AddOddsApiClientWithDependencies_WithoutApiKey_FailsStartupValidation")]
    public void AddOddsApiClientWithDependencies_WithMissingSetting_FailsStartupValidation(string? leagues,
        string? apiKey, string setting)
    {
        // Arrange
        using var provider = BuildProvider(leagues, apiKey);

        // Act
        var action = () => provider.GetRequiredService<IStartupValidator>().Validate();

        // Assert
        action.Should().Throw<OptionsValidationException>()
            .Which.Failures.Should().ContainSingle().Which.Should().StartWith(setting);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_WithEverySettingInvalid_ReportsEveryFailureAtOnce()
    {
        // Arrange
        using var provider = BuildProvider(leagues: null, apiKey: null, baseUrl: "not a url");

        // Act
        var action = () => provider.GetRequiredService<IStartupValidator>().Validate();

        // Assert
        action.Should().Throw<OptionsValidationException>().Which.Failures.Should().HaveCount(3);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_WithLeaguesAndApiKey_BindsThemToOptions()
    {
        // Arrange
        using var provider = BuildProvider(leagues: "soccer_epl;soccer_spain_la_liga", apiKey: "key");

        // Act
        var options = provider.GetRequiredService<IOptions<OddsApiClientOptions>>().Value;

        // Assert
        using var scope = new AssertionScope();

        options.Leagues.Should().BeEquivalentTo("soccer_epl", "soccer_spain_la_liga");
        options.ApiKey.Should().Be("key");
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingHttpClient_ReturnsTransientInstance()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        var first = provider.GetRequiredService<HttpClient>();
        var second = provider.GetRequiredService<HttpClient>();

        // Assert
        second.Should().NotBeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingQuotaLoggingHandler_ReturnsTransientInstance()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        var first = provider.GetRequiredService<QuotaLoggingHandler>();
        var second = provider.GetRequiredService<QuotaLoggingHandler>();

        // Assert
        second.Should().NotBeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingClient_ReturnsTransientInstance()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        var first = provider.GetRequiredService<IClient>();
        var second = provider.GetRequiredService<IClient>();

        // Assert
        using var scope = new AssertionScope();

        first.Should().BeOfType<Client>();
        second.Should().NotBeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_WithoutBaseUrl_ResolvesClientForOddsApi()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        var client = (Client)provider.GetRequiredService<IClient>();

        // Assert
        client.BaseUrl.Should().Be("https://api.the-odds-api.com/");
    }

    [Test]
    public void AddOddsApiClientWithDependencies_WithBaseUrl_ResolvesClientForBaseUrl()
    {
        // Arrange
        using var provider = BuildProvider(baseUrl: "http://localhost:8080");

        // Act
        var client = (Client)provider.GetRequiredService<IClient>();

        // Assert
        client.BaseUrl.Should().Be("http://localhost:8080/");
    }

    [Test]
    public void AddOddsApiClientWithDependencies_WithInvalidBaseUrl_FailsStartupValidation()
    {
        // Arrange
        using var provider = BuildProvider(baseUrl: "not a url");

        // Act
        var action = () => provider.GetRequiredService<IStartupValidator>().Validate();

        // Assert
        action.Should().Throw<OptionsValidationException>()
            .Which.Failures.Should().ContainSingle().Which.Should().StartWith("OddsApiClient:BaseUrl");
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingUpcomingEventsClient_ReturnsTransientInstance()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        var first = provider.GetRequiredService<IUpcomingEventsClient>();
        var second = provider.GetRequiredService<IUpcomingEventsClient>();

        // Assert
        using var scope = new AssertionScope();

        first.Should().BeOfType<UpcomingEventsClient>();
        second.Should().NotBeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingEventResultsClient_ReturnsTransientInstance()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        var first = provider.GetRequiredService<IEventResultsClient>();
        var second = provider.GetRequiredService<IEventResultsClient>();

        // Assert
        using var scope = new AssertionScope();

        first.Should().BeOfType<EventResultsClient>();
        second.Should().NotBeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingOriginalUpcomingEventConverter_ReturnsSingletonInstance()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        var first = provider.GetRequiredService<IOriginalUpcomingEventConverter>();
        var second = provider.GetRequiredService<IOriginalUpcomingEventConverter>();

        // Assert
        using var scope = new AssertionScope();

        first.Should().BeOfType<OriginalUpcomingEventConverter>();
        second.Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingBookmakerConverter_ReturnsSingletonInstance()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        var first = provider.GetRequiredService<IBookmakerConverter>();
        var second = provider.GetRequiredService<IBookmakerConverter>();

        // Assert
        using var scope = new AssertionScope();

        first.Should().BeOfType<BookmakerConverter>();
        second.Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingMarketConverter_ReturnsSingletonInstance()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        var first = provider.GetRequiredService<IMarketConverter>();
        var second = provider.GetRequiredService<IMarketConverter>();

        // Assert
        using var scope = new AssertionScope();

        first.Should().BeOfType<MarketConverter>();
        second.Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingOddConverter_ReturnsSingletonInstance()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        var first = provider.GetRequiredService<IOddConverter>();
        var second = provider.GetRequiredService<IOddConverter>();

        // Assert
        using var scope = new AssertionScope();

        first.Should().BeOfType<OddConverter>();
        second.Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingOriginalCompletedEventConverter_ReturnsSingletonInstance()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        var first = provider.GetRequiredService<IOriginalCompletedEventConverter>();
        var second = provider.GetRequiredService<IOriginalCompletedEventConverter>();

        // Assert
        using var scope = new AssertionScope();

        first.Should().BeOfType<OriginalCompletedEventConverter>();
        second.Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingOutcomeConverter_ReturnsSingletonInstance()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        var first = provider.GetRequiredService<IOutcomeConverter>();
        var second = provider.GetRequiredService<IOutcomeConverter>();

        // Assert
        using var scope = new AssertionScope();

        first.Should().BeOfType<OutcomeConverter>();
        second.Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingScoreModelsConverter_ReturnsSingletonInstance()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        var first = provider.GetRequiredService<IScoreModelsConverter>();
        var second = provider.GetRequiredService<IScoreModelsConverter>();

        // Assert
        using var scope = new AssertionScope();

        first.Should().BeOfType<ScoreModelsConverter>();
        second.Should().BeSameAs(first);
    }

    [Test]
    public void AddOddsApiClientWithDependencies_ResolvingScoreModelConverter_ReturnsSingletonInstance()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        var first = provider.GetRequiredService<IScoreModelConverter>();
        var second = provider.GetRequiredService<IScoreModelConverter>();

        // Assert
        using var scope = new AssertionScope();

        first.Should().BeOfType<ScoreModelConverter>();
        second.Should().BeSameAs(first);
    }

    [TestCase(HttpStatusCode.ServiceUnavailable, TestName = "IClient_WithServiceUnavailableOnce_RetriesTheRequest")]
    [TestCase(HttpStatusCode.TooManyRequests, TestName = "IClient_WithTooManyRequestsOnce_RetriesTheRequest")]
    [TestCase(HttpStatusCode.RequestTimeout, TestName = "IClient_WithRequestTimeoutOnce_RetriesTheRequest")]
    public async Task IClient_WithTransientFailureOnce_RetriesTheRequest(HttpStatusCode transientStatusCode)
    {
        // Arrange
        var handlerMock = new SequenceHttpMessageHandler(transientStatusCode, HttpStatusCode.OK);

        using var provider = BuildProvider(primaryHandler: handlerMock);

        var client = provider.GetRequiredService<IClient>();

        // Act
        var scores = await client.ScoresAsync(League, ApiKey, DaysFrom, CancellationToken.None);

        // Assert
        using var scope = new AssertionScope();

        scores.Should().BeEmpty();
        handlerMock.CallCount.Should().Be(2);
    }

    [Test]
    public async Task IClient_WithPersistentServiceUnavailable_GivesUpAfterThreeRetries()
    {
        // Arrange
        var handlerMock = new SequenceHttpMessageHandler(HttpStatusCode.ServiceUnavailable);

        using var provider = BuildProvider(primaryHandler: handlerMock);

        var client = provider.GetRequiredService<IClient>();

        // Act
        var action = () => client.ScoresAsync(League, ApiKey, DaysFrom, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ApiException>();
        handlerMock.CallCount.Should().Be(4);
    }

    [TestCase(HttpStatusCode.Unauthorized, TestName = "IClient_WithUnauthorized_DoesNotRetry")]
    [TestCase(HttpStatusCode.UnprocessableEntity, TestName = "IClient_WithUnprocessableEntity_DoesNotRetry")]
    public async Task IClient_WithPermanentFailure_DoesNotRetry(HttpStatusCode permanentStatusCode)
    {
        // Arrange
        var handlerMock = new SequenceHttpMessageHandler(permanentStatusCode, HttpStatusCode.OK);

        using var provider = BuildProvider(primaryHandler: handlerMock);

        var client = provider.GetRequiredService<IClient>();

        // Act
        var action = () => client.ScoresAsync(League, ApiKey, DaysFrom, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ApiException>();
        handlerMock.CallCount.Should().Be(1);
    }
}
