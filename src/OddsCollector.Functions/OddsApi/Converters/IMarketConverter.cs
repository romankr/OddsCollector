using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal interface IMarketConverter
{
    Conversion<Odd> GetOdd(ICollection<Markets2>? markets, string? bookmaker, string homeTeam, string awayTeam);
}
