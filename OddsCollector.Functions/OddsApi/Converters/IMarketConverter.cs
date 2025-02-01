using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal interface IMarketConverter
{
    Odd ToOdd(ICollection<Markets2>? markets, string? bookmaker, string awayTeam, string homeTeam);
}
