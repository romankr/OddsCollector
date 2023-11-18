using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OddsCollector.Common.KeyVault.Client;
using OddsCollector.Common.KeyVault.Configuration;
using OddsCollector.Common.OddsApi.Client;
using OddsCollector.Common.OddsApi.Configuration;
using OddsCollector.Common.OddsApi.Converter;
using OddsCollector.Common.OddsApi.WebApi;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // workaround for https://github.com/MicrosoftDocs/azure-docs/issues/32962
        services.Configure<KeyVaultOptions>(o =>
        {
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            o.Name = Environment.GetEnvironmentVariable("KeyVault:Name");
        });
        services.Configure<OddsApiOptions>(o =>
        {
            o.Leagues = Environment.GetEnvironmentVariable("OddsApi:Leagues").Split(";").ToHashSet();
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        });
        services.AddHttpClient<Client>();
        services.AddSingleton<IClient, Client>();
        services.AddSingleton<IOddsClient, OddsClient>();
        services.AddSingleton<IOddsApiObjectConverter, OddsApiObjectConverter>();
        services.AddSingleton<IKeyVaultClient, KeyVaultClient>();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
