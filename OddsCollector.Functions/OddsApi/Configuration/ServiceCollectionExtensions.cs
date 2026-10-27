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
                    o.AddLeagues(configuration[OddsApiClientOptions.LeaguesKey]);
                    o.ApiKey = configuration[OddsApiClientOptions.ApiKeyKey];
                    o.BaseUrl = configuration[OddsApiClientOptions.BaseUrlKey];
                })
                .ValidateOnStart();

            services.AddSingleton<IValidateOptions<OddsApiClientOptions>, OddsApiClientOptionsValidator>();

            services.AddTransient<QuotaLoggingHandler>();

            var clientBuilder = services.AddHttpClient<IClient, Client>((httpClient, provider) =>
                new Client(httpClient)
                {
                    BaseUrl = new Uri(provider.GetRequiredService<IOptions<OddsApiClientOptions>>().Value.BaseUrl)
                        .AbsoluteUri
                });

            // The only retry level for The Odds API: a failed request is retried here, on transient errors only.
            // The timer functions are not retried as a whole, which would repeat every league's request and
            // multiply the calls on top of these retries; the next timer run collects the data again.
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
