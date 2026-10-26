using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal interface IOutcomeConverter
{
    OutcomeType GetOutcome(ICollection<ScoreModel>? scores, string? homeTeam, string? awayTeam);
}
