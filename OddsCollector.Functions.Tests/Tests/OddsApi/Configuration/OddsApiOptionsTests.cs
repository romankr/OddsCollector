using OddsCollector.Functions.OddsApi.Configuration;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Configuration;

[Parallelizable(ParallelScope.All)]
internal sealed class OddsApiOptionsTests
{
    [Test]
    public void SetLeagues_WithValidLeague_ReturnsThisLeague()
    {
        const string league = nameof(league);

        var options = new OddsApiClientOptions();
        options.AddLeagues(league);

        options.Leagues.Should().NotBeNull().And.HaveCount(1);
        options.Leagues.ElementAt(0).Should().NotBeNull().And.Be(league);
    }

    [Test]
    public void SetLeagues_WithValidLeagues_ReturnsTheseLeagues()
    {
        const string leagues = "league1;league2";

        var options = new OddsApiClientOptions();
        options.AddLeagues(leagues);

        options.Leagues.Should().NotBeNull().And.HaveCount(2);
        options.Leagues.ElementAt(0).Should().NotBeNull().And.Be("league1");
        options.Leagues.ElementAt(1).Should().NotBeNull().And.Be("league2");
    }

    [Test]
    public void SetLeagues_WithOneEmptyLeague_ReturnsNonEmptyLeagues()
    {
        const string leagues = "league1;;league2";

        var options = new OddsApiClientOptions();
        options.AddLeagues(leagues);

        options.Leagues.Should().NotBeNull().And.HaveCount(2);
        options.Leagues.ElementAt(0).Should().NotBeNull().And.Be("league1");
        options.Leagues.ElementAt(1).Should().NotBeNull().And.Be("league2");
    }

    [Test]
    public void SetLeagues_WithDuplicatedLeagues_ReturnsNewInstance()
    {
        const string leagues = "league1;league1";

        var options = new OddsApiClientOptions();
        options.AddLeagues(leagues);

        options.Leagues.Should().NotBeNull().And.HaveCount(1);
        options.Leagues.ElementAt(0).Should().NotBeNull().And.Be("league1");
    }

    [TestCase("")]
    [TestCase(null)]
    public void SetLeagues_WithNullOrEmptyLeagues_ThrowsException(string? leaguesString)
    {
        var action = () =>
        {
            var options = new OddsApiClientOptions();
            options.AddLeagues(leaguesString);
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(leaguesString));
    }

    [Test]
    public void SetApiKey_WithValidKey_ReturnsThisKey()
    {
        const string key = nameof(key);

        var options = new OddsApiClientOptions();
        options.SetApiKey(key);

        options.ApiKey.Should().NotBeNull().And.Be(key);
    }

    [TestCase("")]
    [TestCase(null)]
    public void SetApiKey_WithNullOrEmptyApiKey_ThrowsException(string? apiKey)
    {
        var action = () =>
        {
            var options = new OddsApiClientOptions();
            options.SetApiKey(apiKey);
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(apiKey));
    }
}
