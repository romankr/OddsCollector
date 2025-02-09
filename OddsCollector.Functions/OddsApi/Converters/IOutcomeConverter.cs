using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal interface IOutcomeConverter
{
    Odd ToOdd(ICollection<Outcome>? outcomes, string? bookmaker, string awayTeam, string homeTeam);
}
