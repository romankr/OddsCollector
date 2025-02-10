using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class ScoreModelConverter : IScoreModelConverter
{
    public EventScore ToEventScore(ScoreModel scoreModel)
    {
        ArgumentException.ThrowIfNullOrEmpty(scoreModel.Name);

        if (!int.TryParse(scoreModel.Score, out var score))
        {
            throw new ArgumentException(
                $"{nameof(scoreModel)} must have an integer score. Actual score: {scoreModel.Score}",
                nameof(scoreModel.Score));
        }

        return new EventScore { Score = score, Name = scoreModel.Name };
    }
}
