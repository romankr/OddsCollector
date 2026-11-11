using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class OutcomeConverter(IScoreModelsConverter converter) : IOutcomeConverter
{
    public Conversion<OutcomeType> GetOutcome(ICollection<ScoreModel>? scores, string homeTeam, string awayTeam)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeTeam);
        ArgumentException.ThrowIfNullOrWhiteSpace(awayTeam);

        return converter.Convert(scores) switch
        {
            EventScore[] eventScores => GetOutcome(eventScores, homeTeam, awayTeam),
            Rejection rejection => rejection
        };
    }

    private static Conversion<OutcomeType> GetOutcome(EventScore[] eventScores, string homeTeam, string awayTeam)
    {
        var homeScore = eventScores.FirstOrDefault(s => s.Name == homeTeam);

        if (homeScore is null)
        {
            return new Rejection($"no score of {homeTeam}");
        }

        var awayScore = eventScores.FirstOrDefault(s => s.Name == awayTeam);

        if (awayScore is null)
        {
            return new Rejection($"no score of {awayTeam}");
        }

        if (homeScore.Score == awayScore.Score)
        {
            return OutcomeType.Draw;
        }

        return homeScore.Score > awayScore.Score ? OutcomeType.HomeTeam : OutcomeType.AwayTeam;
    }
}
