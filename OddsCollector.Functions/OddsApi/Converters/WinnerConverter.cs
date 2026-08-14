using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal class WinnerConverter(IScoreModelsConverter converter) : IWinnerConverter
{
    public string GetWinner(ICollection<ScoreModel>? scores)
    {
        ArgumentNullException.ThrowIfNull(scores);

        var convertedScores = converter.Convert(scores).ToList();

        if (convertedScores[0].Score == convertedScores[1].Score)
        {
            return OutcomeTypes.Draw;
        }

        return convertedScores[0].Score > convertedScores[1].Score
            ? convertedScores[0].Name
            : convertedScores[1].Name;
    }
}
