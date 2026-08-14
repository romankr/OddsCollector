using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal sealed class MarketConverter
{
    [Test]
    public void ToOdd_WithNullBookmakers_ThrowsException()
    {
        var outcomeConverter = Substitute.For<FunctionApp.IOddConverter>();

        var markerConverter = new FunctionApp.MarketConverter(outcomeConverter);

        var action = () => markerConverter.ToOdd(null, "bookmaker", "awayTeam", "homeTeam");

        action.Should().Throw<ArgumentNullException>().WithParameterName("markets");
    }
}
