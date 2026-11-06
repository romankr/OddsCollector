using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal interface IScoreModelsConverter
{
    IEnumerable<EventScore> Convert(ICollection<ScoreModel>? scores);
}
