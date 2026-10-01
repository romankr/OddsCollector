using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Configuration;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddOddsApiClientWithDependencies(IConfiguration configuration)
        {
            services.AddOptions<OddsApiClientOptions>()
                .Configure(o =>
                {
                    o.AddLeagues(configuration["OddsApiClient:Leagues"]);
                    o.SetApiKey(configuration["OddsApiClient:ApiKey"]);
                    o.SetBaseUrl(configuration["OddsApiClient:BaseUrl"]);
                })
                .ValidateOnStart();

            services.AddTransient<QuotaLoggingHandler>();

            var clientBuilder = services.AddHttpClient<IClient, Client>((httpClient, provider) =>
                new Client(httpClient)
                {
                    BaseUrl = provider.GetRequiredService<IOptions<OddsApiClientOptions>>().Value.BaseUrl.AbsoluteUri
                });

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

            services.TryAddSingleton(TimeProvider.System);
        }
    }
}
