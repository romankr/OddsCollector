using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OddsCollector.Common.KeyVault.Client;
using OddsCollector.Common.KeyVault.Secrets;
using OddsCollector.Common.OddsApi.Client;
using OddsCollector.Common.OddsApi.Configuration;
using OddsCollector.Common.OddsApi.Converter;
using OddsCollector.Common.OddsApi.WebApi;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.Configure<OddsApiClientOptions>(o =>
        {
            // workaround for https://github.com/MicrosoftDocs/azure-docs/issues/32962
            o.SetLeagues(Environment.GetEnvironmentVariable("OddsApiClient:Leagues"));
        });
        services.AddHttpClient<Client>();
        services.AddSingleton<IClient, Client>();
        services.AddSingleton<IOddsApiObjectConverter, OddsApiObjectConverter>();
        services.AddSingleton(
            // workaround for https://github.com/MicrosoftDocs/azure-docs/issues/32962
            SecretClientFactory.CreateSecretClient(Environment.GetEnvironmentVariable("KeyVault:Name"))
        );
        services.AddSingleton<IKeyVaultClient, KeyVaultClient>();
        services.AddSingleton<IOddsApiClient, OddsApiClient>();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
