using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class MarketConverter(IOutcomeConverter converter) : IMarketConverter
{
    public Odd ToOdd(ICollection<Markets2>? markets, string? bookmaker, string awayTeam, string homeTeam)
    {
        ArgumentNullException.ThrowIfNull(markets);

        var market = markets.First(m => m.Key == Markets2Key.H2h);

        return converter.ToOdd(market.Outcomes, bookmaker, awayTeam, homeTeam);
    }
}
