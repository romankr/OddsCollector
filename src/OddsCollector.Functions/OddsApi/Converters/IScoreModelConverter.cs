using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal interface IScoreModelConverter
{
    EventScore ToEventScore(ScoreModel scoreModel);
}
