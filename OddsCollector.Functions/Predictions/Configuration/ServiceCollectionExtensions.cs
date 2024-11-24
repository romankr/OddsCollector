using Microsoft.Extensions.DependencyInjection;

namespace OddsCollector.Functions.Predictions.Configuration;

internal static class ServiceCollectionExtensions
{
    public static void AddPredictionStrategy(this IServiceCollection services)
    {
        services.AddSingleton<IPredictionStrategy, PredictionStrategy>();
        services.AddSingleton<IWinnerFinder, WinnerFinder>();
        services.AddSingleton<IScoreCalculator, ScoreCalculator>();
    }
}
