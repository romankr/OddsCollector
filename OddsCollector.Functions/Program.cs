﻿using System.Runtime.CompilerServices;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.Strategies.Configuration;

[assembly: InternalsVisibleTo("OddsCollector.Functions.Tests")]
// DynamicProxyGenAssembly2 is a temporary assembly built by mocking systems that use CastleProxy
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace OddsCollector.Functions;

internal static class Program
{
    private static void Main()
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices(services =>
            {
                services.AddPredictionStrategy();
                services.AddOddsApiClientWithDependencies();
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();
            })
            .Build();

        host.Run();
    }
}
