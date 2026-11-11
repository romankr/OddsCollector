using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal interface IOddConverter
{
    Conversion<Odd> ToOdd(ICollection<Outcome>? outcomes, string? bookmaker, string homeTeam, string awayTeam);
}
