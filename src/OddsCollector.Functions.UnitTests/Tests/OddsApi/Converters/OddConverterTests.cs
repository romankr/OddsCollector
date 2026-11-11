using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.UnitTests.Tests.OddsApi.Converters;

internal sealed class OddConverterTests
{
    private const string DrawOutcomeName = "Draw";

    private const string Bookmaker = "bookmaker";
    private const string AwayTeam = "awayTeam";
    private const string HomeTeam = "homeTeam";
    private const double Price = 1.1;

    [Test]
    public void ToOdd_WithNullOutcomes_ReturnsRejection()
    {
        // Arrange
        var converter = new OddConverter();

        // Act
        var conversion = converter.ToOdd(null, Bookmaker, HomeTeam, AwayTeam);

        // Assert
        conversion.Value.Should().Be(new Rejection("no outcomes"));
    }

    [TestCase("", TestName = "ToOdd_WithEmptyBookmaker_ReturnsRejection")]
    [TestCase(null, TestName = "ToOdd_WithNullBookmaker_ReturnsRejection")]
    [TestCase(" ", TestName = "ToOdd_WithWhitespaceBookmaker_ReturnsRejection")]
    public void ToOdd_WithNullOrEmptyBookmaker_ReturnsRejection(string? bookmaker)
    {
        // Arrange
        var converter = new OddConverter();

        // Act
        var conversion = converter.ToOdd([], bookmaker, HomeTeam, AwayTeam);

        // Assert
        conversion.Value.Should().Be(new Rejection("no bookmaker key"));
    }

    [Test]
    public void ToOdd_WithoutAwayTeamOutcome_ReturnsRejectionNamingIt()
    {
        // Arrange
        var converter = new OddConverter();

        Outcome[] outcomes =
        [
            new() { Name = HomeTeam, Price = Price },
            new() { Name = DrawOutcomeName, Price = Price }
        ];

        // Act
        var conversion = converter.ToOdd(outcomes, Bookmaker, HomeTeam, AwayTeam);

        // Assert
        conversion.Value.Should().Be(new Rejection($"no {AwayTeam} outcome"));
    }

    [Test]
    public void ToOdd_WithoutHomeTeamOutcome_ReturnsRejectionNamingIt()
    {
        // Arrange
        var converter = new OddConverter();

        Outcome[] outcomes =
        [
            new() { Name = AwayTeam, Price = Price },
            new() { Name = DrawOutcomeName, Price = Price }
        ];

        // Act
        var conversion = converter.ToOdd(outcomes, Bookmaker, HomeTeam, AwayTeam);

        // Assert
        conversion.Value.Should().Be(new Rejection($"no {HomeTeam} outcome"));
    }

    [Test]
    public void ToOdd_WithoutDrawOutcome_ReturnsRejectionNamingIt()
    {
        // Arrange
        var converter = new OddConverter();

        Outcome[] outcomes =
        [
            new() { Name = AwayTeam, Price = Price },
            new() { Name = HomeTeam, Price = Price }
        ];

        // Act
        var conversion = converter.ToOdd(outcomes, Bookmaker, HomeTeam, AwayTeam);

        // Assert
        conversion.Value.Should().Be(new Rejection($"no {DrawOutcomeName} outcome"));
    }

    [Test]
    public void ToOdd_WithOutcomeWithoutPrice_ReturnsRejectionNamingIt()
    {
        // Arrange
        var converter = new OddConverter();

        Outcome[] outcomes =
        [
            new() { Name = AwayTeam, Price = Price },
            new() { Name = HomeTeam, Price = Price },
            new() { Name = DrawOutcomeName, Price = null }
        ];

        // Act
        var conversion = converter.ToOdd(outcomes, Bookmaker, HomeTeam, AwayTeam);

        // Assert
        conversion.Value.Should().Be(new Rejection($"no price for {DrawOutcomeName}"));
    }

    [TestCase(1, TestName = "ToOdd_WithPriceOfOne_ReturnsRejectionNamingIt")]
    [TestCase(0.5, TestName = "ToOdd_WithPriceBelowOne_ReturnsRejectionNamingIt")]
    [TestCase(0, TestName = "ToOdd_WithZeroPrice_ReturnsRejectionNamingIt")]
    [TestCase(-2, TestName = "ToOdd_WithNegativePrice_ReturnsRejectionNamingIt")]
    [TestCase(double.NaN, TestName = "ToOdd_WithNaNPrice_ReturnsRejectionNamingIt")]
    [TestCase(double.PositiveInfinity, TestName = "ToOdd_WithInfinitePrice_ReturnsRejectionNamingIt")]
    public void ToOdd_WithUnusablePrice_ReturnsRejectionNamingIt(double drawPrice)
    {
        // Arrange
        var converter = new OddConverter();

        Outcome[] outcomes =
        [
            new() { Name = AwayTeam, Price = Price },
            new() { Name = HomeTeam, Price = Price },
            new() { Name = DrawOutcomeName, Price = drawPrice }
        ];

        // Act
        var conversion = converter.ToOdd(outcomes, Bookmaker, HomeTeam, AwayTeam);

        // Assert
        conversion.Value.Should().BeOfType<Rejection>()
            .Which.Reason.Should().StartWith($"unusable price for {DrawOutcomeName}: ");
    }

    [Test]
    public void ToOdd_WithAllPrices_ReturnsOddForBookmaker()
    {
        // Arrange
        const double homePrice = 1.5;
        const double drawPrice = 3.5;
        const double awayPrice = 6.0;

        var converter = new OddConverter();

        Outcome[] outcomes =
        [
            new() { Name = AwayTeam, Price = awayPrice },
            new() { Name = HomeTeam, Price = homePrice },
            new() { Name = DrawOutcomeName, Price = drawPrice }
        ];

        // Act
        var conversion = converter.ToOdd(outcomes, Bookmaker, HomeTeam, AwayTeam);

        // Assert
        conversion.Value.Should().Be(new Odd { Bookmaker = Bookmaker, Home = homePrice, Draw = drawPrice, Away = awayPrice });
    }
}
