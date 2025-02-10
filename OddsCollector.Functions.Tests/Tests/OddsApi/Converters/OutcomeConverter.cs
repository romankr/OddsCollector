using OddsCollector.Functions.OddsApi.WebApi;
using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal sealed class OutcomeConverter
{
    [Test]
    public void ToOdd_WithNullBookmakers_ThrowsException()
    {
        var converter = new FunctionApp.OutcomeConverter();

        var action = () => converter.ToOdd(null, "bookmaker", "awayTeam", "homeTeam");

        action.Should().Throw<ArgumentNullException>().WithParameterName("outcomes");
    }

    [TestCase("", TestName = "ToOdds_WithEmptyBookmaker_ThrowsException")]
    [TestCase(null, TestName = "ToOdds_WithNullBookmaker_ThrowsException")]
    public void ToOdds_WithNullOrEmptyBookmaker_ThrowsException(string? bookmaker)
    {
        var converter = new FunctionApp.OutcomeConverter();

        var action = () => converter.ToOdd([], bookmaker, "awayTeam", "homeTeam");

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(bookmaker));
    }

    [Test]
    public void ToOdds_WithoutAwayTeam_ThrowsException()
    {
        var converter = new FunctionApp.OutcomeConverter();

        var action = () => converter.ToOdd([
                new Outcome { Name = "homeTeam", Price = 1.1 },
                new Outcome { Name = "Draw", Price = 1.1 }
            ],
            "bookmaker",
            "awayTeam",
            "homeTeam");

        action.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void ToOdds_WithoutHomeTeam_ThrowsException()
    {
        var converter = new FunctionApp.OutcomeConverter();

        var action = () => converter.ToOdd([
                new Outcome { Name = "awayTeam", Price = 1.1 },
                new Outcome { Name = "Draw", Price = 1.1 }
            ],
            "bookmaker",
            "awayTeam",
            "homeTeam");

        action.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void ToOdds_WithoutDraw_ThrowsException()
    {
        var converter = new FunctionApp.OutcomeConverter();

        var action = () => converter.ToOdd([
                new Outcome { Name = "awayTeam", Price = 1.1 },
                new Outcome { Name = "homeTeam", Price = 1.1 }
            ],
            "bookmaker",
            "awayTeam",
            "homeTeam");

        action.Should().Throw<InvalidOperationException>();
    }
}
