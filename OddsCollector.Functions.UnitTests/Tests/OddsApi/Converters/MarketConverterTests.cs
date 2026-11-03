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
    public void TryGetOdd_WithNullMarkets_ThrowsArgumentNullException()
    {
        // Arrange
        var marketConverter = new MarketConverter(Substitute.For<IOddConverter>());

        // Act
        var action = () => marketConverter.TryGetOdd(null, Bookmaker, HomeTeam, AwayTeam, out _);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("markets");
    }

    [Test]
    public void TryGetOdd_WithEmptyMarkets_ReturnsFalse()
    {
        // Arrange
        var marketConverter = new MarketConverter(Substitute.For<IOddConverter>());

        // Act
        var found = marketConverter.TryGetOdd([], Bookmaker, HomeTeam, AwayTeam, out var odd);

        // Assert
        using var scope = new AssertionScope();

        found.Should().BeFalse();
        odd.Should().BeNull();
    }

    [Test]
    public void TryGetOdd_WithoutHeadToHeadMarket_ReturnsFalseWithoutConvertingOutcomes()
    {
        // Arrange
        var oddConverterMock = Substitute.For<IOddConverter>();

        var marketConverter = new MarketConverter(oddConverterMock);

        Markets2[] markets = [new() { Key = Markets2Key.Totals, Outcomes = [] }];

        // Act
        var found = marketConverter.TryGetOdd(markets, Bookmaker, HomeTeam, AwayTeam, out var odd);

        // Assert
        using var scope = new AssertionScope();

        found.Should().BeFalse();
        odd.Should().BeNull();
        oddConverterMock.ReceivedCalls().Should().BeEmpty();
    }

    [Test]
    public void TryGetOdd_WithHeadToHeadMarket_ReturnsOddOfItsOutcomes()
    {
        // Arrange
        var expectedOdd = new Odd { Bookmaker = Bookmaker, Home = 1.5, Draw = 3.5, Away = 4.0 };

        Outcome[] headToHeadOutcomes = [new() { Name = HomeTeam, Price = 1.5 }];

        var oddConverterStub = Substitute.For<IOddConverter>();
        oddConverterStub.ToOdd(headToHeadOutcomes, Bookmaker, HomeTeam, AwayTeam).Returns(expectedOdd);

        var marketConverter = new MarketConverter(oddConverterStub);

        Markets2[] markets =
        [
            new() { Key = Markets2Key.Totals, Outcomes = [] },
            new() { Key = Markets2Key.H2h, Outcomes = headToHeadOutcomes }
        ];

        // Act
        var found = marketConverter.TryGetOdd(markets, Bookmaker, HomeTeam, AwayTeam, out var odd);

        // Assert
        using var scope = new AssertionScope();

        found.Should().BeTrue();
        odd.Should().BeSameAs(expectedOdd);
    }
}
