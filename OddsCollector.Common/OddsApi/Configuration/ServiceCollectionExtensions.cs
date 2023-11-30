using Microsoft.Extensions.DependencyInjection;
using OddsCollector.Common.OddsApi.Converter;
using OddsCollector.Common.OddsApi.WebApi;

namespace OddsCollector.Common.OddsApi.Configuration;

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
