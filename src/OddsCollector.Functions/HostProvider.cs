using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OddsCollector.Functions.OddsApi;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.Predictions.Configuration;
using OddsCollector.Functions.ServiceBus;
using OpenTelemetry;

namespace OddsCollector.Functions;

internal static class HostProvider
{
    public static IHost CreateHost(string[] args)
    {
        var builder = FunctionsApplication.CreateBuilder(args);

        builder.Services.AddPredictionStrategy();
        builder.Services.AddOpenTelemetry()
            .UseFunctionsWorkerDefaults()
            .WithMetrics(metrics => metrics.AddMeter(OddsApiMetrics.MeterName, DeadLetterMetrics.MeterName))
            .UseAzureMonitorExporter();
        builder.Services.AddOddsApiClientWithDependencies(builder.Configuration);
        builder.Services.AddMetrics();
        builder.Services.TryAddSingleton<DeadLetterMetrics>();

        return builder.Build();
    }
}
