using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class BookmakerConverter(IMarketConverter converter) : IBookmakerConverter
{
    public IEnumerable<Odd> ToOdds(ICollection<Bookmakers>? bookmakers, string? awayTeam, string? homeTeam)
    {
        ArgumentNullException.ThrowIfNull(bookmakers);
        ArgumentException.ThrowIfNullOrEmpty(awayTeam);
        ArgumentException.ThrowIfNullOrEmpty(homeTeam);

        foreach (var bookmaker in bookmakers)
        {
            yield return converter.ToOdd(bookmaker.Markets, bookmaker.Key, awayTeam, homeTeam);
        }
    }
}
