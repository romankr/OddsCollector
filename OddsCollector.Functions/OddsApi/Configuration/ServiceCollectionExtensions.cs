using Microsoft.Extensions.DependencyInjection;
using OddsCollector.Functions.OddsApi.Clients;
using OddsCollector.Functions.OddsApi.Converters;
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
        services.AddSingleton<IOddsConverter, OddsConverter>();
        services.AddSingleton<IScoresConverter, ScoresConverter>();
        services.AddSingleton<IUpcomingEventsClient, UpcomingEventsClient>();
        services.AddSingleton<IEventResultsClient, EventResultsClient>();
    }
}
