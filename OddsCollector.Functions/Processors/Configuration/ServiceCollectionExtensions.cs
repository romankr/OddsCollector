using Microsoft.Extensions.DependencyInjection;

namespace OddsCollector.Functions.Processors.Configuration;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddFunctionProcessors()
        {
            services.AddSingleton<IPredictionProcessor, PredictionProcessor>();
            services.AddSingleton<IPredictionHttpRequestProcessor, PredictionHttpRequestProcessor>();
        }
    }
}
