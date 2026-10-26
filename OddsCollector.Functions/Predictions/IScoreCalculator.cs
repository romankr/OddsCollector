using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal interface IScoreCalculator
{
    OutcomeScore[] GetScores(IReadOnlyCollection<Odd> odds);
}
