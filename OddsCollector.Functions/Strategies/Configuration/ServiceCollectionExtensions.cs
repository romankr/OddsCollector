using Microsoft.Extensions.DependencyInjection;

namespace OddsCollector.Functions.Strategies.Configuration;

internal static class ServiceCollectionExtensions
{
    public static void AddPredictionStrategy(this IServiceCollection services)
    {
        services.AddSingleton<IPredictionStrategy, AdjustedConsensusStrategy>();
    }
}
