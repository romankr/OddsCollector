using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.Predictions.Configuration;

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
                services.AddOpenTelemetry()
                    .UseFunctionsWorkerDefaults()
                    .UseAzureMonitorExporter();
                services.AddOddsApiClientWithDependencies(
                    Environment.GetEnvironmentVariable("OddsApiClient:Leagues"),
                    Environment.GetEnvironmentVariable("OddsApiClient:ApiKey"));
            })
            .Build();
    }
}
