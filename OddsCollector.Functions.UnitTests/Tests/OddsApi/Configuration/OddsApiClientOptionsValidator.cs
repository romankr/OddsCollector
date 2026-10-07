using FluentAssertions.Execution;
using FunctionApp = OddsCollector.Functions.OddsApi.Configuration;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Configuration;

internal sealed class OddsApiClientOptionsValidator
{
    private static FunctionApp.OddsApiClientOptions CreateValidOptions()
    {
        var options = new FunctionApp.OddsApiClientOptions { ApiKey = "key" };
        options.AddLeagues("soccer_epl");

        return options;
    }

    [Test]
    public void Validate_WithValidOptions_Succeeds()
    {
        var result = new FunctionApp.OddsApiClientOptionsValidator().Validate(null, CreateValidOptions());

        result.Succeeded.Should().BeTrue();
    }

    [TestCase("http://localhost:8080")]
    [TestCase("https://api.the-odds-api.com/")]
    public void Validate_WithHttpBaseUrl_Succeeds(string baseUrl)
    {
        var options = CreateValidOptions();
        options.BaseUrl = baseUrl;

        var result = new FunctionApp.OddsApiClientOptionsValidator().Validate(null, options);

        result.Succeeded.Should().BeTrue();
    }

    [Test]
    public void Validate_WithoutLeagues_FailsNamingTheSetting()
    {
        var options = new FunctionApp.OddsApiClientOptions { ApiKey = "key" };

        var result = new FunctionApp.OddsApiClientOptionsValidator().Validate(null, options);

        result.Failures.Should().ContainSingle().Which.Should().StartWith("OddsApiClient:Leagues must name");
    }

    [TestCase("")]
    [TestCase(" ")]
    public void Validate_WithoutApiKey_FailsNamingTheSetting(string apiKey)
    {
        var options = CreateValidOptions();
        options.ApiKey = apiKey;

        var result = new FunctionApp.OddsApiClientOptionsValidator().Validate(null, options);

        result.Failures.Should().ContainSingle().Which.Should().Be("OddsApiClient:ApiKey is required");
    }

    [TestCase("not a url")]
    [TestCase("/v4/sports")]
    [TestCase("ftp://localhost")]
    public void Validate_WithInvalidBaseUrl_FailsWithTheValue(string baseUrl)
    {
        var options = CreateValidOptions();
        options.BaseUrl = baseUrl;

        var result = new FunctionApp.OddsApiClientOptionsValidator().Validate(null, options);

        result.Failures.Should().ContainSingle().Which.Should()
            .Be($"OddsApiClient:BaseUrl must be an absolute HTTP(S) URL. Actual value: {baseUrl}");
    }

    [Test]
    public void Validate_WithEverySettingInvalid_ReportsEveryFailure()
    {
        var options = new FunctionApp.OddsApiClientOptions { BaseUrl = "not a url" };

        var result = new FunctionApp.OddsApiClientOptionsValidator().Validate(null, options);

        using var scope = new AssertionScope();

        result.Failed.Should().BeTrue();
        result.Failures.Should().HaveCount(3);
    }
}
