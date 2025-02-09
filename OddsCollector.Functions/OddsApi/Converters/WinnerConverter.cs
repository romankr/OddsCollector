using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal class WinnerConverter(IScoreModelsConverter converter) : IWinnerConverter
{
    public string GetWinner(ICollection<ScoreModel>? scores)
    {
        var convertedScores = converter.Convert(scores);

        if (convertedScores.ElementAt(0).Score == convertedScores.ElementAt(1).Score)
        {
            return OutcomeTypes.Draw;
        }

        return convertedScores.ElementAt(0).Score > convertedScores.ElementAt(1).Score
            ? convertedScores.ElementAt(0).Name
            : convertedScores.ElementAt(1).Name;
    }
}
