using FunctionApp = OddsCollector.Functions.OddsApi.Configuration;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Configuration;

internal sealed class OddsApiClientOptions
{
    [Test]
    public void SetLeagues_WithOneLeague_ReturnsOneLeague()
    {
        var options = new FunctionApp.OddsApiClientOptions();

        options.AddLeagues("league");

        options.Leagues.Should().NotBeNull().And.BeEquivalentTo("league");
    }

    [Test]
    public void SetLeagues_WithMultipleLeagues_ReturnsMultipleLeagues()
    {
        var options = new FunctionApp.OddsApiClientOptions();

        options.AddLeagues("league1;league2");

        options.Leagues.Should().NotBeNull().And.BeEquivalentTo("league1", "league2");
    }

    [Test]
    public void SetLeagues_WithDuplicateLeagues_ReturnsOnlyOneLeague()
    {
        var options = new FunctionApp.OddsApiClientOptions();

        options.AddLeagues("league1;league1");

        options.Leagues.Should().NotBeNull().And.BeEquivalentTo("league1");
    }

    [Test]
    public void SetLeagues_WithEmptyLeague_ReturnsNoLeagues()
    {
        var options = new FunctionApp.OddsApiClientOptions();

        options.AddLeagues(";;");

        options.Leagues.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void SetLeagues_WithLeadingAndTrailingCharacters_ReturnsCorrectLeagues()
    {
        var options = new FunctionApp.OddsApiClientOptions();

        options.AddLeagues("league1\n; league2\r");

        options.Leagues.Should().NotBeNull().And.BeEquivalentTo("league1", "league2");
    }

    [Test]
    public void SetLeagues_WithNullOrEmptyLeagues_ThrowsException()
    {
        var options = new FunctionApp.OddsApiClientOptions();

        var action = () => options.AddLeagues(string.Empty);

        action.Should().ThrowExactly<ArgumentException>().WithParameterName("leagues");
    }

    [Test]
    public void SetLeagues_WithNullLeagues_ThrowsException()
    {
        var options = new FunctionApp.OddsApiClientOptions();

        var action = () => options.AddLeagues(null);

        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("leagues");
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
    public void SetApiKey_WithNullApiKey_ThrowsException()
    {
        var options = new FunctionApp.OddsApiClientOptions();

        var action = () => options.SetApiKey(null);

        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("apiKey");
    }

    [Test]
    public void SetApiKey_WithEmptyApiKey_ThrowsException()
    {
        var options = new FunctionApp.OddsApiClientOptions();

        var action = () => options.SetApiKey(string.Empty);

        action.Should().ThrowExactly<ArgumentException>().WithParameterName("apiKey");
    }
}
