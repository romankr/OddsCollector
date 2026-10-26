using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.UnitTests.Tests.OddsApi.Converters;

internal sealed class OddConverterTests
{
    // The name The Odds API gives the draw outcome of a head-to-head market.
    private const string DrawOutcomeName = "Draw";

    private const string Bookmaker = "bookmaker";
    private const string AwayTeam = "awayTeam";
    private const string HomeTeam = "homeTeam";
    private const double Price = 1.1;

    [Test]
    public void ToOdd_WithNullOutcomes_ThrowsArgumentNullException()
    {
        // Arrange
        var converter = new OddConverter();

        // Act
        var action = () => converter.ToOdd(null, Bookmaker, HomeTeam, AwayTeam);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("outcomes");
    }

    [TestCase("", TestName = "ToOdd_WithEmptyBookmaker_ThrowsArgumentException")]
    [TestCase(null, TestName = "ToOdd_WithNullBookmaker_ThrowsArgumentException")]
    [TestCase(" ", TestName = "ToOdd_WithWhitespaceBookmaker_ThrowsArgumentException")]
    public void ToOdd_WithNullOrEmptyBookmaker_ThrowsArgumentException(string? bookmaker)
    {
        // Arrange
        var converter = new OddConverter();

        // Act
        var action = () => converter.ToOdd([], bookmaker, HomeTeam, AwayTeam);

        // Assert
        action.Should().Throw<ArgumentException>().WithParameterName(nameof(bookmaker));
    }

    [Test]
    public void ToOdd_WithoutAwayTeamOutcome_ThrowsArgumentExceptionNamingIt()
    {
        // Arrange
        var converter = new OddConverter();

        Outcome[] outcomes =
        [
            new() { Name = HomeTeam, Price = Price },
            new() { Name = DrawOutcomeName, Price = Price }
        ];

        // Act
        var action = () => converter.ToOdd(outcomes, Bookmaker, HomeTeam, AwayTeam);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage($"Outcome {AwayTeam} is missing*").And.ParamName.Should().Be("outcomes");
    }

    [Test]
    public void ToOdd_WithoutHomeTeamOutcome_ThrowsArgumentExceptionNamingIt()
    {
        // Arrange
        var converter = new OddConverter();

        Outcome[] outcomes =
        [
            new() { Name = AwayTeam, Price = Price },
            new() { Name = DrawOutcomeName, Price = Price }
        ];

        // Act
        var action = () => converter.ToOdd(outcomes, Bookmaker, HomeTeam, AwayTeam);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage($"Outcome {HomeTeam} is missing*").And.ParamName.Should().Be("outcomes");
    }

    [Test]
    public void ToOdd_WithoutDrawOutcome_ThrowsArgumentExceptionNamingIt()
    {
        // Arrange
        var converter = new OddConverter();

        Outcome[] outcomes =
        [
            new() { Name = AwayTeam, Price = Price },
            new() { Name = HomeTeam, Price = Price }
        ];

        // Act
        var action = () => converter.ToOdd(outcomes, Bookmaker, HomeTeam, AwayTeam);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage($"Outcome {DrawOutcomeName} is missing*").And.ParamName.Should().Be("outcomes");
    }

    [Test]
    public void ToOdd_WithOutcomeWithoutPrice_ThrowsArgumentExceptionNamingIt()
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
        var action = () => converter.ToOdd(outcomes, Bookmaker, HomeTeam, AwayTeam);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage($"Outcome {DrawOutcomeName} has no price*").And.ParamName.Should().Be("outcomes");
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
        var odd = converter.ToOdd(outcomes, Bookmaker, HomeTeam, AwayTeam);

        // Assert
        odd.Should().Be(new Odd { Bookmaker = Bookmaker, Home = homePrice, Draw = drawPrice, Away = awayPrice });
    }
}
