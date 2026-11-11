using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal interface IBookmakerConverter
{
    IEnumerable<Odd> ToOdds(IEnumerable<Bookmakers> bookmakers, string homeTeam, string awayTeam);
}
