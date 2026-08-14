using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class ScoreModelsConverter(IScoreModelConverter converter) : IScoreModelsConverter
{
    public IEnumerable<EventScore> Convert(ICollection<ScoreModel>? scores)
    {
        CheckParameters(scores);

        foreach (var scoreModel in scores!)
        {
            yield return converter.ToEventScore(scoreModel);
        }
    }

    private static void CheckParameters(ICollection<ScoreModel>? scores)
    {
        ArgumentNullException.ThrowIfNull(scores);

        if (scores.Count != 2)
        {
            throw new ArgumentException($"{nameof(scores)} must have 2 elements", nameof(scores));
        }
    }
}
