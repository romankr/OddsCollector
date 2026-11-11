using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class ScoreModelsConverter(IScoreModelConverter converter) : IScoreModelsConverter
{
    private const int ExpectedScoreCount = 2;

    public Conversion<EventScore[]> Convert(ICollection<ScoreModel>? scores)
    {
        if (scores is null)
        {
            return new Rejection("no scores");
        }

        if (scores.Count != ExpectedScoreCount)
        {
            return new Rejection($"expected {ExpectedScoreCount} scores, got {scores.Count}");
        }

        List<EventScore> eventScores = [with(capacity: ExpectedScoreCount)];

        foreach (var scoreModel in scores)
        {
            switch (converter.ToEventScore(scoreModel))
            {
                case EventScore eventScore:
                    eventScores.Add(eventScore);
                    break;
                case Rejection rejection:
                    return rejection;
            }
        }

        return eventScores.ToArray();
    }
}
