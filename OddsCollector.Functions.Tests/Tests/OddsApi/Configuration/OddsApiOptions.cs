using OddsCollector.Functions.OddsApi.Configuration;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Configuration;

internal class OddsApiOptions
{
    private static readonly IEnumerable<(string LeagueString, HashSet<string> ExpectedLeagues)>
        TestCases = new List<(string LeagueString, HashSet<string> ExpectedLeagues)>
        {
            ("league", ["league"]),
            ("league1;league2", ["league1", "league2"]),
            ("league1;;league2", ["league1", "league2"]),
            ("league1;league1", ["league1"])
        };

    [TestCaseSource(nameof(TestCases))]
    public void SetLeagues_WithValidLeagueInputString_ReturnsCorrectLeagues(
        (string LeagueString, HashSet<string> ExpectedLeagues) testCase)
    {
        // Arrange
        var options = new OddsApiClientOptions();

        // Act
        options.AddLeagues(testCase.LeagueString);

        // Assert
        options.Leagues.Should().NotBeNull().And.BeEquivalentTo(testCase.ExpectedLeagues);
    }

    [Test]
    public void SetLeagues_WithNullOrEmptyLeagues_ThrowsException()
    {
        // Arrange
        var options = new OddsApiClientOptions();

        // Act
        var action = () => options.AddLeagues(string.Empty);

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("leaguesString");
    }

    [Test]
    public void SetLeagues_WithNullLeagues_ThrowsException()
    {
        // Arrange
        var options = new OddsApiClientOptions();

        // Act
        var action = () => options.AddLeagues(null);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("leaguesString");
    }

    [Test]
    public void SetApiKey_WithValidKey_ReturnsThisKey()
    {
        // Arrange
        const string key = nameof(key);
        var options = new OddsApiClientOptions();

        // Act
        options.SetApiKey(key);

        // Assert
        options.ApiKey.Should().NotBeNull().And.Be(key);
    }

    [Test]
    public void SetApiKey_WithNullApiKey_ThrowsException()
    {
        // Arrange
        var options = new OddsApiClientOptions();

        // Act
        var action = () => options.SetApiKey(null);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("apiKey");
    }

    [Test]
    public void SetApiKey_WithEmptyApiKey_ThrowsException()
    {
        // Arrange
        var options = new OddsApiClientOptions();

        // Act
        var action = () => options.SetApiKey(string.Empty);

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("apiKey");
    }
}
