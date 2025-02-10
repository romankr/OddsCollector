using OddsCollector.Functions.OddsApi.WebApi;
using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal sealed class MarketConverter
{
    [Test]
    public void ToOdd_WithNullBookmakers_ThrowsException()
    {
        var outcomeConverter = Substitute.For<FunctionApp.IOutcomeConverter>();

        var markerConverter = new FunctionApp.MarketConverter(outcomeConverter);

        var action = () => markerConverter.ToOdd(null, "bookmaker", "awayTeam", "homeTeam");

        action.Should().Throw<ArgumentNullException>().WithParameterName("markets");
    }

    [Test]
    public void ToOdd_WithEmptyBookmakers_ThrowsException()
    {
        var outcomeConverter = Substitute.For<FunctionApp.IOutcomeConverter>();

        var markerConverter = new FunctionApp.MarketConverter(outcomeConverter);

        var action = () => markerConverter.ToOdd([], "bookmaker", "awayTeam", "homeTeam");

        action.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void ToOdd_WithUnexpectedMarket_ThrowsException()
    {
        var outcomeConverter = Substitute.For<FunctionApp.IOutcomeConverter>();

        var markerConverter = new FunctionApp.MarketConverter(outcomeConverter);

        var action = () =>
            markerConverter.ToOdd([new Markets2 { Key = Markets2Key.Totals }], "bookmaker", "awayTeam", "homeTeam");

        action.Should().Throw<InvalidOperationException>();
    }
}
