using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal interface IScoreModelConverter
{
    Conversion<EventScore> ToEventScore(ScoreModel scoreModel);
}
