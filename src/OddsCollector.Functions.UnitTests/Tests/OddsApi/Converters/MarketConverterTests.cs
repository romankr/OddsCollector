using FluentAssertions.Execution;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.UnitTests.Tests.OddsApi.Converters;

internal sealed class MarketConverterTests
{
    private const string Bookmaker = "bookmaker";
    private const string AwayTeam = "awayTeam";
    private const string HomeTeam = "homeTeam";

    [Test]
    public void GetOdd_WithNullMarkets_ReturnsRejection()
    {
        // Arrange
        var marketConverter = new MarketConverter(Substitute.For<IOddConverter>());

        // Act
        var conversion = marketConverter.GetOdd(null, Bookmaker, HomeTeam, AwayTeam);

        // Assert
        conversion.Value.Should().Be(new Rejection("no head-to-head market"));
    }

    [Test]
    public void GetOdd_WithEmptyMarkets_ReturnsRejection()
    {
        // Arrange
        var marketConverter = new MarketConverter(Substitute.For<IOddConverter>());

        // Act
        var conversion = marketConverter.GetOdd([], Bookmaker, HomeTeam, AwayTeam);

        // Assert
        conversion.Value.Should().Be(new Rejection("no head-to-head market"));
    }

    [Test]
    public void GetOdd_WithoutHeadToHeadMarket_ReturnsRejectionWithoutConvertingOutcomes()
    {
        // Arrange
        var oddConverterMock = Substitute.For<IOddConverter>();

        var marketConverter = new MarketConverter(oddConverterMock);

        Markets2[] markets = [new() { Key = Markets2Key.Totals, Outcomes = [] }];

        // Act
        var conversion = marketConverter.GetOdd(markets, Bookmaker, HomeTeam, AwayTeam);

        // Assert
        using var scope = new AssertionScope();

        conversion.Value.Should().Be(new Rejection("no head-to-head market"));
        oddConverterMock.ReceivedCalls().Should().BeEmpty();
    }

    [Test]
    public void GetOdd_WithHeadToHeadMarket_ReturnsOddOfItsOutcomes()
    {
        // Arrange
        var expectedOdd = new Odd { Bookmaker = Bookmaker, Home = 1.5, Draw = 3.5, Away = 4.0 };

        Conversion<Odd> converted = expectedOdd;

        Outcome[] headToHeadOutcomes = [new() { Name = HomeTeam, Price = 1.5 }];

        var oddConverterStub = Substitute.For<IOddConverter>();
        oddConverterStub.ToOdd(headToHeadOutcomes, Bookmaker, HomeTeam, AwayTeam).Returns(converted);

        var marketConverter = new MarketConverter(oddConverterStub);

        Markets2[] markets =
        [
            new() { Key = Markets2Key.Totals, Outcomes = [] },
            new() { Key = Markets2Key.H2h, Outcomes = headToHeadOutcomes }
        ];

        // Act
        var conversion = marketConverter.GetOdd(markets, Bookmaker, HomeTeam, AwayTeam);

        // Assert
        conversion.Value.Should().BeSameAs(expectedOdd);
    }
}
