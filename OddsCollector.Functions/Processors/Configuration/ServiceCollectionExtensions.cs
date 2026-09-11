using Microsoft.Extensions.DependencyInjection;

namespace OddsCollector.Functions.Processors.Configuration;

internal static class ServiceCollectionExtensions
{
    public static void AddFunctionProcessors(this IServiceCollection services)
    {
        services.AddTransient<IEventResultProcessor, EventResultProcessor>();
        services.AddTransient<IUpcomingEventsProcessor, UpcomingEventsProcessor>();
        services.AddSingleton<IPredictionProcessor, PredictionProcessor>();
        services.AddSingleton<IPredictionHttpRequestProcessor, PredictionHttpRequestProcessor>();
    }
}
