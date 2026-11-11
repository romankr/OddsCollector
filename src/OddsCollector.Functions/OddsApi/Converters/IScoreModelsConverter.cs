using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal interface IScoreModelsConverter
{
    Conversion<EventScore[]> Convert(ICollection<ScoreModel>? scores);
}
