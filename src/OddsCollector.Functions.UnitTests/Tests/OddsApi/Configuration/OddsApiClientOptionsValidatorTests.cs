using FluentAssertions.Execution;
using OddsCollector.Functions.OddsApi.Configuration;

namespace OddsCollector.Functions.UnitTests.Tests.OddsApi.Configuration;

internal sealed class OddsApiClientOptionsValidatorTests
{
    private static OddsApiClientOptions CreateValidOptions()
    {
        var options = new OddsApiClientOptions { ApiKey = "key" };
        options.AddLeagues("soccer_epl");

        return options;
    }

    [Test]
    public void Validate_WithValidOptions_Succeeds()
    {
        // Arrange
        var validator = new OddsApiClientOptionsValidator();

        // Act
        var result = validator.Validate(null, CreateValidOptions());

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [TestCase("http://localhost:8080", TestName = "Validate_WithHttpBaseUrl_Succeeds")]
    [TestCase("https://api.the-odds-api.com/", TestName = "Validate_WithHttpsBaseUrl_Succeeds")]
    public void Validate_WithHttpOrHttpsBaseUrl_Succeeds(string baseUrl)
    {
        // Arrange
        var options = CreateValidOptions();
        options.BaseUrl = baseUrl;

        var validator = new OddsApiClientOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Test]
    public void Validate_WithoutLeagues_FailsNamingTheSetting()
    {
        // Arrange
        var options = new OddsApiClientOptions { ApiKey = "key" };

        var validator = new OddsApiClientOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failures.Should().ContainSingle().Which.Should().StartWith("OddsApiClient:Leagues must name");
    }

    [TestCase("", TestName = "Validate_WithEmptyApiKey_FailsNamingTheSetting")]
    [TestCase(" ", TestName = "Validate_WithWhitespaceApiKey_FailsNamingTheSetting")]
    public void Validate_WithoutApiKey_FailsNamingTheSetting(string apiKey)
    {
        // Arrange
        var options = CreateValidOptions();
        options.ApiKey = apiKey;

        var validator = new OddsApiClientOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failures.Should().ContainSingle().Which.Should().Be("OddsApiClient:ApiKey is required");
    }

    [TestCase("not a url", TestName = "Validate_WithTextBaseUrl_FailsWithTheValue")]
    [TestCase("/v4/sports", TestName = "Validate_WithRelativeBaseUrl_FailsWithTheValue")]
    [TestCase("ftp://localhost", TestName = "Validate_WithNonHttpBaseUrl_FailsWithTheValue")]
    public void Validate_WithInvalidBaseUrl_FailsWithTheValue(string baseUrl)
    {
        // Arrange
        var options = CreateValidOptions();
        options.BaseUrl = baseUrl;

        var validator = new OddsApiClientOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failures.Should().ContainSingle().Which.Should()
            .Be($"OddsApiClient:BaseUrl must be an absolute HTTP(S) URL. Actual value: {baseUrl}");
    }

    [Test]
    public void Validate_WithEverySettingInvalid_ReportsEveryFailure()
    {
        // Arrange
        var options = new OddsApiClientOptions { BaseUrl = "not a url" };

        var validator = new OddsApiClientOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        using var scope = new AssertionScope();

        result.Failed.Should().BeTrue();
        result.Failures.Should().HaveCount(3);
    }

    [TestCase("soccer_epl;soccer_spain_la_liga", TestName = "Validate_WithSeveralFootballLeagues_Succeeds")]
    [TestCase("soccer_uefa_champs_league", TestName = "Validate_WithOneFootballLeague_Succeeds")]
    public void Validate_WithFootballLeagues_Succeeds(string leagues)
    {
        // Arrange
        var options = new OddsApiClientOptions { ApiKey = "key" };
        options.AddLeagues(leagues);

        var validator = new OddsApiClientOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [TestCase("basketball_nba", "basketball_nba",
        TestName = "Validate_WithNonFootballLeague_FailsNamingIt")]
    [TestCase("soccer_epl;icehockey_nhl;basketball_nba", "basketball_nba;icehockey_nhl",
        TestName = "Validate_WithFootballAndNonFootballLeagues_FailsNamingOnlyTheNonFootballOnes")]
    [TestCase("Soccer_EPL", "Soccer_EPL",
        TestName = "Validate_WithUpperCaseFootballPrefix_FailsBecauseSportKeysAreLowerCase")]
    [TestCase("soccer", "soccer",
        TestName = "Validate_WithPrefixWithoutSeparator_FailsNamingIt")]
    public void Validate_WithNonFootballLeagues_FailsNamingThem(string leagues, string expectedUnsupported)
    {
        // Arrange
        var options = new OddsApiClientOptions { ApiKey = "key" };
        options.AddLeagues(leagues);

        var validator = new OddsApiClientOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failures.Should().ContainSingle().Which.Should().Be(
            "OddsApiClient:Leagues supports only football (soccer_*) sport keys. " +
            $"Unsupported: {expectedUnsupported}");
    }
}
