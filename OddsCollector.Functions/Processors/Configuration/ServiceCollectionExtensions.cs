using Microsoft.Extensions.DependencyInjection;

namespace OddsCollector.Functions.Processors.Configuration;

internal static class ServiceCollectionExtensions
{
    public static void AddFunctionProcessors(this IServiceCollection services)
    {
        services.AddSingleton<IEventResultProcessor, EventResultProcessor>();
        services.AddSingleton<IUpcomingEventsProcessor, UpcomingEventsProcessor>();
        services.AddSingleton<IPredictionProcessor, PredictionProcessor>();
        services.AddSingleton<IPredictionHttpRequestProcessor, PredictionHttpRequestProcessor>();
    }
}
