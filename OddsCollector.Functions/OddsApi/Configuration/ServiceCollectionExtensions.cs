using Microsoft.Extensions.DependencyInjection;
using OddsCollector.Functions.OddsApi.Converter;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOddsApiClientWithDependencies(this IServiceCollection services)
    {
        services.Configure<OddsApiClientOptions>(o =>
        {
            // workaround for https://github.com/MicrosoftDocs/azure-docs/issues/32962
            o.AddLeagues(Environment.GetEnvironmentVariable("OddsApiClient:Leagues"));
            o.SetApiKey(Environment.GetEnvironmentVariable("OddsApiClient:ApiKey"));
        });

        services.AddHttpClient<Client>();
        services.AddSingleton<IClient, Client>();
        services.AddSingleton<IOddsApiObjectConverter, OddsApiObjectConverter>();
        services.AddSingleton<IOddsApiClient, OddsApiClient>();

        return services;
    }
}
