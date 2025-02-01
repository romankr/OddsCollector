using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal interface IBookmakerConverter
{
    IEnumerable<Odd> ToOdds(ICollection<Bookmakers>? bookmakers, string? awayTeam, string? homeTeam);
}
