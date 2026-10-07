using System.Globalization;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class ScoreModelConverter : IScoreModelConverter
{
    public EventScore ToEventScore(ScoreModel scoreModel)
    {
        if (!int.TryParse(scoreModel.Score, NumberStyles.Integer, CultureInfo.InvariantCulture, out var score))
        {
            throw new ArgumentException(
                $"{nameof(scoreModel)} must have an integer score. Actual score: {scoreModel.Score}",
                nameof(scoreModel));
        }

        // A missing name is rejected by EventScore.
        return new EventScore { Name = scoreModel.Name ?? string.Empty, Score = score };
    }
}
