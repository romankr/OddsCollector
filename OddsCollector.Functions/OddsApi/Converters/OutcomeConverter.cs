using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class OutcomeConverter(IScoreModelsConverter converter) : IOutcomeConverter
{
    public OutcomeType GetOutcome(ICollection<ScoreModel>? scores, string? homeTeam, string? awayTeam)
    {
        ArgumentNullException.ThrowIfNull(scores);
        ArgumentException.ThrowIfNullOrWhiteSpace(homeTeam);
        ArgumentException.ThrowIfNullOrWhiteSpace(awayTeam);

        var eventScores = converter.Convert(scores).ToList();

        var homeScore = GetScore(eventScores, homeTeam, nameof(scores));
        var awayScore = GetScore(eventScores, awayTeam, nameof(scores));

        if (homeScore == awayScore)
        {
            return OutcomeType.Draw;
        }

        return homeScore > awayScore ? OutcomeType.HomeTeam : OutcomeType.AwayTeam;
    }

    private static int GetScore(IEnumerable<EventScore> eventScores, string team, string paramName)
    {
        // Scores are matched to the teams by name, so a score for a team that is not playing is never counted.
        var eventScore = eventScores.FirstOrDefault(s => s.Name == team)
                         ?? throw new ArgumentException($"Score of {team} is missing", paramName);

        return eventScore.Score;
    }
}
