using Microsoft.Extensions.DependencyInjection;

namespace OddsCollector.Functions.Predictions.Configuration;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddPredictionStrategy()
        {
            services.AddSingleton<IPredictionStrategy, PredictionStrategy>();
            services.AddSingleton<IWinnerFinder, WinnerFinder>();
            services.AddSingleton<IScoreCalculator, ScoreCalculator>();
        }
    }
}
