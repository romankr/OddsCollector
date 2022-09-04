namespace OddsCollector.Betting;

using Strategies;

/// <summary>
/// Extension methods for the dependency injection.
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers betting strategies in the dependency injection.
    /// </summary>
    /// <param name="services">A service collection <see cref="IServiceCollection"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is null.</exception>
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
        services.AddSingleton<IBettingStrategy, RandomStrategy>();
    }
}