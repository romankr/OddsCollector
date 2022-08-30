namespace OddsCollector.Betting;

using Strategies;

internal static class ServiceCollectionExtensions
{
    public static void AddBettingStrategies(this IServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddSingleton<IBettingStrategy, SimpleConsensusStrategy>();
        services.AddSingleton<IBettingStrategy, AdjustedConsensusStrategy>();
        services.AddSingleton<IBettingStrategy, LaLigaSimpleConsensusStrategy>();
        services.AddSingleton<IBettingStrategy, EplSimpleConsensusStrategy>();
        services.AddSingleton<IBettingStrategy, BundesligaSimpleConsensusStrategy>();
    }
}