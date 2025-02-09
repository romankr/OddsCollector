using OddsCollector.Functions.OddsApi.Converters;
using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal sealed class BookmakerConverter
{
    [Test]
    public void ToOdds_WithNullBookmakers_ThrowsException()
    {
        var marketConverter = Substitute.For<IMarketConverter>();

        var bookmakerConverter = new FunctionApp.BookmakerConverter(marketConverter);

        var action = () => bookmakerConverter.ToOdds(null, "awayTeam", "homeTeam").ToList();

        action.Should().Throw<ArgumentNullException>().WithParameterName("bookmakers");
    }

    [TestCase("", TestName = "ToOdds_WithEmptyAwayTeam_ThrowsException")]
    [TestCase(null, TestName = "ToOdds_WithNullAwayTeam_ThrowsException")]
    public void ToOdds_WithNullOrEmptyAwayTeam_ThrowsException(string? awayTeam)
    {
        var marketConverter = Substitute.For<IMarketConverter>();

        var bookmakerConverter = new FunctionApp.BookmakerConverter(marketConverter);

        var action = () => bookmakerConverter.ToOdds([], awayTeam, "homeTeam").ToList();

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(awayTeam));
    }

    [TestCase("", TestName = "ToOdds_WithEmptyHomeTeam_ThrowsException")]
    [TestCase(null, TestName = "ToOdds_WithNullHomeTeam_ThrowsException")]
    public void ToOdds_WithNullOrEmptyHomeTeam_ThrowsException(string? homeTeam)
    {
        var marketConverter = Substitute.For<IMarketConverter>();

        var bookmakerConverter = new FunctionApp.BookmakerConverter(marketConverter);

        var action = () => bookmakerConverter.ToOdds([], "awayTeam", homeTeam).ToList();

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(homeTeam));
    }
}
