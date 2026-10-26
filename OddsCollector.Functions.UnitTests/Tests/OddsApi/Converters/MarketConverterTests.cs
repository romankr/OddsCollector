using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.UnitTests.Tests.OddsApi.Converters;

internal sealed class MarketConverterTests
{
    private const string Bookmaker = "bookmaker";
    private const string AwayTeam = "awayTeam";
    private const string HomeTeam = "homeTeam";

    [Test]
    public void ToOdd_WithNullMarkets_ThrowsArgumentNullException()
    {
        // Arrange
        var marketConverter = new MarketConverter(Substitute.For<IOddConverter>());

        // Act
        var action = () => marketConverter.ToOdd(null, Bookmaker, AwayTeam, HomeTeam);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("markets");
    }

    [Test]
    public void ToOdd_WithEmptyMarkets_ThrowsInvalidOperationException()
    {
        // Arrange
        var marketConverter = new MarketConverter(Substitute.For<IOddConverter>());

        // Act
        var action = () => marketConverter.ToOdd([], Bookmaker, AwayTeam, HomeTeam);

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void ToOdd_WithoutHeadToHeadMarket_ThrowsInvalidOperationException()
    {
        // Arrange
        var marketConverter = new MarketConverter(Substitute.For<IOddConverter>());

        Markets2[] markets = [new() { Key = Markets2Key.Totals }];

        // Act
        var action = () => marketConverter.ToOdd(markets, Bookmaker, AwayTeam, HomeTeam);

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }
}
