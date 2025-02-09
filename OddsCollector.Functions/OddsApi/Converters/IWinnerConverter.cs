using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal interface IWinnerConverter
{
    string GetWinner(ICollection<ScoreModel>? scores);
}
