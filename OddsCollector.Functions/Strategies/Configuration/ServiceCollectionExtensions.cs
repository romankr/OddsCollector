using Microsoft.Extensions.DependencyInjection;

namespace OddsCollector.Functions.Strategies.Configuration;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPredictionStrategy(this IServiceCollection services)
    {
        services.AddSingleton<IPredictionStrategy, AdjustedConsensusStrategy>();

        return services;
    }
}
