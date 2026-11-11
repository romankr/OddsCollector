using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class MarketConverter(IOddConverter converter) : IMarketConverter
{
    public Conversion<Odd> GetOdd(ICollection<Markets2>? markets, string? bookmaker, string homeTeam,
        string awayTeam)
    {
        var market = markets?.FirstOrDefault(m => m.Key == Markets2Key.H2h);

        if (market is null)
        {
            return new Rejection("no head-to-head market");
        }

        return converter.ToOdd(market.Outcomes, bookmaker, homeTeam: homeTeam, awayTeam: awayTeam);
    }
}
