using Microsoft.Extensions.DependencyInjection;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Configuration;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddOddsApiClientWithDependencies(string? leagues, string? apiKey)
        {
            // workaround for https://github.com/MicrosoftDocs/azure-docs/issues/32962
            services.Configure<OddsApiClientOptions>(o =>
            {
                o.AddLeagues(leagues);
                o.SetApiKey(apiKey);
            });

            services.AddTransient<QuotaLoggingHandler>();

            var clientBuilder = services.AddHttpClient<IClient, Client>();

            clientBuilder.AddStandardResilienceHandler();
            clientBuilder.AddHttpMessageHandler<QuotaLoggingHandler>();

            services.AddTransient<IUpcomingEventsClient, UpcomingEventsClient>();
            services.AddTransient<IEventResultsClient, EventResultsClient>();

            services.AddSingleton<IOriginalUpcomingEventConverter, OriginalUpcomingEventConverter>();
            services.AddSingleton<IBookmakerConverter, BookmakerConverter>();
            services.AddSingleton<IMarketConverter, MarketConverter>();
            services.AddSingleton<IOutcomeConverter, OutcomeConverter>();
            services.AddSingleton<IOriginalCompletedEventConverter, OriginalCompletedEventConverter>();
            services.AddSingleton<IWinnerConverter, WinnerConverter>();
            services.AddSingleton<IScoreModelsConverter, ScoreModelsConverter>();
            services.AddSingleton<IScoreModelConverter, ScoreModelConverter>();
        }
    }
}
