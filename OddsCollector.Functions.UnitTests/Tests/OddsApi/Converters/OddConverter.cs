using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;
using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal sealed class OddConverter
{
    private const string Bookmaker = "bookmaker";
    private const string AwayTeam = "awayTeam";
    private const string HomeTeam = "homeTeam";
    private const double Price = 1.1;

    [Test]
    public void ToOdd_WithNullOutcomes_ThrowsArgumentNullException()
    {
        // Arrange
        var converter = new FunctionApp.OddConverter();

        // Act
        var action = () => converter.ToOdd(null, Bookmaker, AwayTeam, HomeTeam);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("outcomes");
    }

    [TestCase("", TestName = "ToOdd_WithEmptyBookmaker_ThrowsArgumentException")]
    [TestCase(null, TestName = "ToOdd_WithNullBookmaker_ThrowsArgumentException")]
    [TestCase(" ", TestName = "ToOdd_WithWhitespaceBookmaker_ThrowsArgumentException")]
    public void ToOdd_WithNullOrEmptyBookmaker_ThrowsArgumentException(string? bookmaker)
    {
        // Arrange
        var converter = new FunctionApp.OddConverter();

        // Act
        var action = () => converter.ToOdd([], bookmaker, AwayTeam, HomeTeam);

        // Assert
        action.Should().Throw<ArgumentException>().WithParameterName(nameof(bookmaker));
    }

    [Test]
    public void ToOdd_WithoutAwayTeamPrice_ThrowsInvalidOperationException()
    {
        // Arrange
        var converter = new FunctionApp.OddConverter();

        Outcome[] outcomes =
        [
            new() { Name = HomeTeam, Price = Price },
            new() { Name = OutcomeTypes.Draw, Price = Price }
        ];

        // Act
        var action = () => converter.ToOdd(outcomes, Bookmaker, AwayTeam, HomeTeam);

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void ToOdd_WithoutHomeTeamPrice_ThrowsInvalidOperationException()
    {
        // Arrange
        var converter = new FunctionApp.OddConverter();

        Outcome[] outcomes =
        [
            new() { Name = AwayTeam, Price = Price },
            new() { Name = OutcomeTypes.Draw, Price = Price }
        ];

        // Act
        var action = () => converter.ToOdd(outcomes, Bookmaker, AwayTeam, HomeTeam);

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void ToOdd_WithoutDrawPrice_ThrowsInvalidOperationException()
    {
        // Arrange
        var converter = new FunctionApp.OddConverter();

        Outcome[] outcomes =
        [
            new() { Name = AwayTeam, Price = Price },
            new() { Name = HomeTeam, Price = Price }
        ];

        // Act
        var action = () => converter.ToOdd(outcomes, Bookmaker, AwayTeam, HomeTeam);

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }
}
