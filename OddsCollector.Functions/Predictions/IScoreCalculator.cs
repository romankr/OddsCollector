using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal interface IScoreCalculator
{
    OutcomeScore[] GetScores(ICollection<Odd> odds);
}
