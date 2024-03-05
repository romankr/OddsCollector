using Microsoft.Extensions.DependencyInjection;
using OddsCollector.Functions.OddsApi.Converter;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Configuration;

internal static class ServiceCollectionExtensions
{
    public static void AddOddsApiClientWithDependencies(this IServiceCollection services, string? leagues,
        string? apiKey)
    {
        services.Configure<OddsApiClientOptions>(o =>
        {
            // workaround for https://github.com/MicrosoftDocs/azure-docs/issues/32962
            o.AddLeagues(leagues);
            o.SetApiKey(apiKey);
        });

        services.AddHttpClient<Client>();
        services.AddSingleton<IClient, Client>();
        services.AddSingleton<IOddsApiObjectConverter, OddsApiObjectConverter>();
        services.AddSingleton<IOddsApiClient, OddsApiClient>();
    }
}
