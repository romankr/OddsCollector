using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Configuration;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddOddsApiClientWithDependencies(IConfiguration configuration)
        {
            // Read through IConfiguration rather than the environment directly, so a setting
            // named OddsApiClient__ApiKey (the only form Linux plans allow) is found as well.
            // ValidateOnStart builds the options when the host starts, so a missing setting
            // stops the app there instead of failing every run.
            services.AddOptions<OddsApiClientOptions>()
                .Configure(o =>
                {
                    o.AddLeagues(configuration["OddsApiClient:Leagues"]);
                    o.SetApiKey(configuration["OddsApiClient:ApiKey"]);
                })
                .ValidateOnStart();

            services.AddTransient<QuotaLoggingHandler>();

            var clientBuilder = services.AddHttpClient<IClient, Client>();

            clientBuilder.AddStandardResilienceHandler();
            clientBuilder.AddHttpMessageHandler<QuotaLoggingHandler>();

            services.AddTransient<IUpcomingEventsClient, UpcomingEventsClient>();
            services.AddTransient<IEventResultsClient, EventResultsClient>();

            services.AddSingleton<IOriginalUpcomingEventConverter, OriginalUpcomingEventConverter>();
            services.AddSingleton<IBookmakerConverter, BookmakerConverter>();
            services.AddSingleton<IMarketConverter, MarketConverter>();
            services.AddSingleton<IOddConverter, OddConverter>();
            services.AddSingleton<IOriginalCompletedEventConverter, OriginalCompletedEventConverter>();
            services.AddSingleton<IOutcomeConverter, OutcomeConverter>();
            services.AddSingleton<IScoreModelsConverter, ScoreModelsConverter>();
            services.AddSingleton<IScoreModelConverter, ScoreModelConverter>();
        }
    }
}
