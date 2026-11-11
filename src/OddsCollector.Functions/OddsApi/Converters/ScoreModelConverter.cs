using System.Globalization;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class ScoreModelConverter : IScoreModelConverter
{
    public Conversion<EventScore> ToEventScore(ScoreModel scoreModel)
    {
        ArgumentNullException.ThrowIfNull(scoreModel);

        if (string.IsNullOrWhiteSpace(scoreModel.Name))
        {
            return new Rejection("a score has no team");
        }

        if (!int.TryParse(scoreModel.Score, NumberStyles.Integer, CultureInfo.InvariantCulture, out var score))
        {
            return new Rejection($"the score of {scoreModel.Name} is not an integer: {scoreModel.Score}");
        }

        return new EventScore { Name = scoreModel.Name, Score = score };
    }
}
