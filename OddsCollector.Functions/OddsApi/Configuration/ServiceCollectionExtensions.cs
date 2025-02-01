using Microsoft.Extensions.DependencyInjection;
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
        services.AddSingleton<IUpcomingEventsClient, UpcomingEventsClient>();
        services.AddSingleton<IOriginalUpcomingEventConverter, OriginalUpcomingEventConverter>();
        services.AddSingleton<IBookmakerConverter, BookmakerConverter>();
        services.AddSingleton<IMarketConverter, MarketConverter>();
        services.AddSingleton<IOutcomeConverter, OutcomeConverter>();
        services.AddSingleton<IEventResultsClient, EventResultsClient>();
        services.AddSingleton<IOriginalCompletedEventConverter, OriginalCompletedEventConverter>();
        services.AddSingleton<IWinnerConverter, WinnerConverter>();
        services.AddSingleton<IScoreModelsConverter, ScoreModelsConverter>();
        services.AddSingleton<IScoreModelConverter, ScoreModelConverter>();
    }
}
