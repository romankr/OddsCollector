using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.Processors.Configuration;
using OddsCollector.Functions.Strategies.Configuration;

namespace OddsCollector.Functions;

internal static class HostProvider
{
    public static IHost Get()
    {
        return new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices(services =>
            {
                services.AddPredictionStrategy();
                services.AddFunctionProcessors();
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();
                services.AddOddsApiClientWithDependencies(
                    Environment.GetEnvironmentVariable("OddsApiClient:Leagues"),
                    Environment.GetEnvironmentVariable("OddsApiClient:ApiKey"));
            })
            .Build();
    }
}
