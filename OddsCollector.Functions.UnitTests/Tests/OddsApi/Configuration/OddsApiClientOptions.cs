using FunctionApp = OddsCollector.Functions.OddsApi.Configuration;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Configuration;

internal sealed class OddsApiClientOptions
{
    [Test]
    public void AddLeagues_WithOneLeague_ReturnsOneLeague()
    {
        var options = new FunctionApp.OddsApiClientOptions();

        options.AddLeagues("league");

        options.Leagues.Should().NotBeNull().And.BeEquivalentTo("league");
    }

    [Test]
    public void AddLeagues_WithMultipleLeagues_ReturnsMultipleLeagues()
    {
        var options = new FunctionApp.OddsApiClientOptions();

        options.AddLeagues("league1;league2");

        options.Leagues.Should().NotBeNull().And.BeEquivalentTo("league1", "league2");
    }

    [Test]
    public void AddLeagues_WithDuplicateLeagues_ReturnsOnlyOneLeague()
    {
        var options = new FunctionApp.OddsApiClientOptions();

        options.AddLeagues("league1;league1");

        options.Leagues.Should().NotBeNull().And.BeEquivalentTo("league1");
    }

    [Test]
    public void AddLeagues_WithEmptyLeague_ReturnsNoLeagues()
    {
        var options = new FunctionApp.OddsApiClientOptions();

        options.AddLeagues(";;");

        options.Leagues.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void AddLeagues_WithLeadingAndTrailingCharacters_ReturnsCorrectLeagues()
    {
        var options = new FunctionApp.OddsApiClientOptions();

        options.AddLeagues("league1\n; league2\r");

        options.Leagues.Should().NotBeNull().And.BeEquivalentTo("league1", "league2");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void AddLeagues_WithoutLeagues_AddsNoLeagues(string? leagues)
    {
        var options = new FunctionApp.OddsApiClientOptions();

        options.AddLeagues(leagues);

        options.Leagues.Should().BeEmpty();
    }

    [Test]
    public void SetApiKey_WithKey_ReturnsKey()
    {
        const string key = nameof(key);
        var options = new FunctionApp.OddsApiClientOptions();

        options.SetApiKey(key);

        options.ApiKey.Should().NotBeNull().And.Be(key);
    }

    [Test]
    public void SetApiKey_WithNullApiKey_SetsEmptyKey()
    {
        var options = new FunctionApp.OddsApiClientOptions();

        options.SetApiKey(null);

        options.ApiKey.Should().BeEmpty();
    }

    [Test]
    public void BaseUrl_ByDefault_IsOddsApi()
    {
        var options = new FunctionApp.OddsApiClientOptions();

        options.BaseUrl.Should().Be("https://api.the-odds-api.com/");
    }

    [Test]
    public void SetBaseUrl_WithUrl_ReturnsUrl()
    {
        var options = new FunctionApp.OddsApiClientOptions();

        options.SetBaseUrl("http://localhost:8080");

        options.BaseUrl.Should().Be("http://localhost:8080");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void SetBaseUrl_WithoutUrl_KeepsDefault(string? baseUrl)
    {
        var options = new FunctionApp.OddsApiClientOptions();

        options.SetBaseUrl(baseUrl);

        options.BaseUrl.Should().Be(FunctionApp.OddsApiClientOptions.DefaultBaseUrl);
    }

    [TestCase("not a url")]
    [TestCase("/v4/sports")]
    [TestCase("ftp://localhost")]
    public void SetBaseUrl_WithInvalidUrl_KeepsItForValidation(string baseUrl)
    {
        var options = new FunctionApp.OddsApiClientOptions();

        options.SetBaseUrl(baseUrl);

        options.BaseUrl.Should().Be(baseUrl);
    }
}
