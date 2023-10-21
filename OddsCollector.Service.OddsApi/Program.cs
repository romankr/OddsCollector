using OddsCollector.Common.Scheduler;
using OddsCollector.Common.ServiceBus;
using OddsCollector.Service.OddsApi.Client;
using OddsCollector.Service.OddsApi.Jobs;
using Quartz;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient<Client>();
        services.AddSingleton<IClient, Client>();
        services.AddSingleton<IOddsClient, OddsClient>();
        services.AddSingleton(ServiceBusCreator.GetServiceBusClient(context.Configuration));

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

host.Run();
