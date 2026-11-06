using OddsCollector.Functions.OddsApi.Configuration;

namespace OddsCollector.Functions.UnitTests.Tests.OddsApi.Configuration;

internal sealed class OddsApiClientOptionsTests
{
    [Test]
    public void AddLeagues_WithOneLeague_ReturnsOneLeague()
    {
        // Arrange
        var options = new OddsApiClientOptions();

        // Act
        options.AddLeagues("league");

        // Assert
        options.Leagues.Should().NotBeNull().And.BeEquivalentTo("league");
    }

    [Test]
    public void AddLeagues_WithMultipleLeagues_ReturnsMultipleLeagues()
    {
        // Arrange
        var options = new OddsApiClientOptions();

        // Act
        options.AddLeagues("league1;league2");

        // Assert
        options.Leagues.Should().NotBeNull().And.BeEquivalentTo("league1", "league2");
    }

    [Test]
    public void AddLeagues_WithDuplicateLeagues_ReturnsOnlyOneLeague()
    {
        // Arrange
        var options = new OddsApiClientOptions();

        // Act
        options.AddLeagues("league1;league1");

        // Assert
        options.Leagues.Should().NotBeNull().And.BeEquivalentTo("league1");
    }

    [Test]
    public void AddLeagues_WithEmptyLeague_ReturnsNoLeagues()
    {
        // Arrange
        var options = new OddsApiClientOptions();

        // Act
        options.AddLeagues(";;");

        // Assert
        options.Leagues.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void AddLeagues_WithLeadingAndTrailingCharacters_ReturnsCorrectLeagues()
    {
        // Arrange
        var options = new OddsApiClientOptions();

        // Act
        options.AddLeagues("league1\n; league2\r");

        // Assert
        options.Leagues.Should().NotBeNull().And.BeEquivalentTo("league1", "league2");
    }

    [TestCase(null, TestName = "AddLeagues_WithNull_AddsNoLeagues")]
    [TestCase("", TestName = "AddLeagues_WithEmptyText_AddsNoLeagues")]
    [TestCase(" ", TestName = "AddLeagues_WithWhitespace_AddsNoLeagues")]
    public void AddLeagues_WithoutLeagues_AddsNoLeagues(string? leagues)
    {
        // Arrange
        var options = new OddsApiClientOptions();

        // Act
        options.AddLeagues(leagues);

        // Assert
        options.Leagues.Should().BeEmpty();
    }

    [Test]
    public void ApiKey_ByDefault_IsEmpty()
    {
        // Arrange
        var options = new OddsApiClientOptions();

        // Act
        var apiKey = options.ApiKey;

        // Assert
        apiKey.Should().BeEmpty();
    }

    [Test]
    public void ApiKey_SetToKey_ReturnsKey()
    {
        // Arrange
        const string key = nameof(key);
        var options = new OddsApiClientOptions();

        // Act
        options.ApiKey = key;

        // Assert
        options.ApiKey.Should().Be(key);
    }

    [Test]
    public void ApiKey_SetToNull_ReturnsEmpty()
    {
        // Arrange
        var options = new OddsApiClientOptions { ApiKey = "key" };

        // Act
        options.ApiKey = null;

        // Assert
        options.ApiKey.Should().BeEmpty();
    }

    [Test]
    public void BaseUrl_ByDefault_IsOddsApi()
    {
        // Arrange
        var options = new OddsApiClientOptions();

        // Act
        var baseUrl = options.BaseUrl;

        // Assert
        baseUrl.Should().Be("https://api.the-odds-api.com/");
    }

    [TestCase("http://localhost:8080", "http://localhost:8080", TestName = "BaseUrl_SetToUrl_ReturnsUrl")]
    [TestCase(" http://localhost:8080\n", "http://localhost:8080",
        TestName = "BaseUrl_SetToUrlWithSurroundingWhitespace_ReturnsTrimmedUrl")]
    public void BaseUrl_SetToUrl_ReturnsTrimmedUrl(string baseUrl, string expectedBaseUrl)
    {
        // Arrange
        var options = new OddsApiClientOptions();

        // Act
        options.BaseUrl = baseUrl;

        // Assert
        options.BaseUrl.Should().Be(expectedBaseUrl);
    }

    [TestCase(null, TestName = "BaseUrl_SetToNull_FallsBackToOddsApi")]
    [TestCase("", TestName = "BaseUrl_SetToEmpty_FallsBackToOddsApi")]
    [TestCase(" ", TestName = "BaseUrl_SetToWhitespace_FallsBackToOddsApi")]
    public void BaseUrl_SetToBlank_FallsBackToOddsApi(string? baseUrl)
    {
        // Arrange
        var options = new OddsApiClientOptions { BaseUrl = "http://localhost:8080" };

        // Act
        options.BaseUrl = baseUrl;

        // Assert
        options.BaseUrl.Should().Be(OddsApiClientOptions.DefaultBaseUrl);
    }

    [TestCase("not a url", TestName = "BaseUrl_SetToText_KeepsItForValidation")]
    [TestCase("/v4/sports", TestName = "BaseUrl_SetToRelativeUrl_KeepsItForValidation")]
    [TestCase("ftp://localhost", TestName = "BaseUrl_SetToNonHttpUrl_KeepsItForValidation")]
    public void BaseUrl_SetToInvalidUrl_KeepsItForValidation(string baseUrl)
    {
        // Arrange
        var options = new OddsApiClientOptions();

        // Act
        options.BaseUrl = baseUrl;

        // Assert
        options.BaseUrl.Should().Be(baseUrl);
    }
}
