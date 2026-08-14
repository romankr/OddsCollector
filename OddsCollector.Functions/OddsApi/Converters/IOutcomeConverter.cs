using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal interface IOutcomeConverter
{
    string GetOutcome(ICollection<ScoreModel>? scores);
}
