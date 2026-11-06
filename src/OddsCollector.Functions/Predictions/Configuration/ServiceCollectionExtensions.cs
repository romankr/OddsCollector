using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OddsCollector.Functions.Predictions.Configuration;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddPredictionStrategy()
        {
            services.AddSingleton<IPredictionStrategy, PredictionStrategy>();
            services.AddSingleton<IOutcomePredictor, OutcomePredictor>();
            services.AddSingleton<IScoreCalculator, ScoreCalculator>();
            services.TryAddSingleton(TimeProvider.System);
        }
    }
}
