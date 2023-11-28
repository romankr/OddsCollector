using FluentAssertions;
using OddsCollector.Common.OddsApi.Configuration;

namespace OddsCollector.Common.Tests.OddsApi.Configuration;

[Parallelizable(ParallelScope.All)]
internal sealed class OddsApiOptionsTests
{
    [Test]
    public void SetLeagues_WithValidLeague_ReturnsValidInstance()
    {
        string league = nameof(league);

        var options = new OddsApiClientOptions();
        options.SetLeagues(league);

        options.Leagues.Should().NotBeNull().And.HaveCount(1);
        options.Leagues.ElementAt(0).Should().NotBeNull().And.Be(league);
    }

    [Test]
    public void SetLeagues_WithValidLeagues_ReturnsValidInstance()
    {
        var leagues = "league1;league2";

        var options = new OddsApiClientOptions();
        options.SetLeagues(leagues);

        options.Leagues.Should().NotBeNull().And.HaveCount(2);
        options.Leagues.ElementAt(0).Should().NotBeNull().And.Be("league1");
        options.Leagues.ElementAt(1).Should().NotBeNull().And.Be("league2");
    }

    [Test]
    public void SetLeagues_WithDuplicatedLeagues_ReturnsNewInstance()
    {
        var leagues = "league1;league1";

        var options = new OddsApiClientOptions();
        options.SetLeagues(leagues);

        options.Leagues.Should().NotBeNull().And.HaveCount(1);
        options.Leagues.ElementAt(0).Should().NotBeNull().And.Be("league1");
    }

    [TestCase("")]
    [TestCase(null)]
    public void SetLeagues_WithNullOrEmptyLeagues_ThrowsException(string? leagues)
    {
        var action = () =>
        {
            var options = new OddsApiClientOptions();
            options.SetLeagues(leagues);
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(leagues));
    }
}
