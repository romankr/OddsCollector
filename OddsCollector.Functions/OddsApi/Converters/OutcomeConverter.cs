using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class OutcomeConverter(IScoreModelsConverter converter) : IOutcomeConverter
{
    public OutcomeType GetOutcome(ICollection<ScoreModel>? scores, string? awayTeam, string? homeTeam)
    {
        ArgumentNullException.ThrowIfNull(scores);
        ArgumentException.ThrowIfNullOrWhiteSpace(awayTeam);
        ArgumentException.ThrowIfNullOrWhiteSpace(homeTeam);

        var eventScores = converter.Convert(scores).ToList();

        var awayScore = GetScore(eventScores, awayTeam, nameof(scores));
        var homeScore = GetScore(eventScores, homeTeam, nameof(scores));

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
