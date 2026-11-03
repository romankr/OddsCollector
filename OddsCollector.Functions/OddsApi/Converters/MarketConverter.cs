using System.Diagnostics.CodeAnalysis;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class MarketConverter(IOddConverter converter) : IMarketConverter
{
    public bool TryGetOdd(ICollection<Markets2>? markets, string? bookmaker, string homeTeam, string awayTeam,
        [NotNullWhen(true)] out Odd? odd)
    {
        ArgumentNullException.ThrowIfNull(markets);

        var market = markets.FirstOrDefault(m => m.Key == Markets2Key.H2h);

        if (market is null)
        {
            odd = null;

            return false;
        }

        odd = converter.ToOdd(market.Outcomes, bookmaker, homeTeam: homeTeam, awayTeam: awayTeam);

        return true;
    }
}
