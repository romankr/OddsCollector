using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class ScoreModelsConverter(IScoreModelConverter converter) : IScoreModelsConverter
{
    private const int ExpectedScoreCount = 2;

    public IEnumerable<EventScore> Convert(ICollection<ScoreModel>? scores)
    {
        ArgumentNullException.ThrowIfNull(scores);

        if (scores.Count != ExpectedScoreCount)
        {
            throw new ArgumentException($"{nameof(scores)} must have {ExpectedScoreCount} elements", nameof(scores));
        }

        return Iterate(scores);
    }

    private IEnumerable<EventScore> Iterate(ICollection<ScoreModel> scores)
    {
        foreach (var scoreModel in scores)
        {
            yield return converter.ToEventScore(scoreModel);
        }
    }
}
