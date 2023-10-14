using OddsCollector.Common.Scheduler;
using OddsCollector.OddsApiService.Client;
using OddsCollector.OddsApiService.Jobs;
using Quartz;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient<Client>();
        services.AddSingleton<IClient, Client>();
        services.AddSingleton<IOddsClient, OddsClient>();
        services.AddSingleton<IConverter, Converter>();
        services.AddSingleton<IDefaultParameters, DefaultParameters>();

        services.AddQuartz(q =>
        {
            q.AddJobConfiguration<UpcomingEventsJob>(context.Configuration);
            q.AddJobConfiguration<EventResultsJob>(context.Configuration);
        });

        services.AddQuartzHostedService(q =>
        {
            q.WaitForJobsToComplete = true;
        });
    })
    .Build();

await host.RunAsync();
