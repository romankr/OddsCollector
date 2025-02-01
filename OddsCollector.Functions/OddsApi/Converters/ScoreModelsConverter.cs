using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class ScoreModelsConverter(IScoreModelConverter converter) : IScoreModelsConverter
{
    public IEnumerable<EventScore> Convert(ICollection<ScoreModel>? scoreModels)
    {
        ArgumentNullException.ThrowIfNull(scoreModels);

        if (scoreModels.Count != 2)
        {
            throw new ArgumentException($"{nameof(scoreModels)} must have 2 elements", nameof(scoreModels));
        }

        foreach (var scoreModel in scoreModels)
        {
            yield return converter.ToEventScore(scoreModel);
        }
    }
}
