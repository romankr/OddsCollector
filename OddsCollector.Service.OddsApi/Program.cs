using OddsCollector.Common.Scheduler;
using OddsCollector.Common.ServiceBus;
using OddsCollector.Service.OddsApi.Client;
using OddsCollector.Service.OddsApi.Jobs;
using OddsCollector.Service.OddsApi.Processor;
using OddsCollector.Service.OddsApi.Vault;
using Quartz;

#pragma warning disable CA1852

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient<Client>();
        services.AddSingleton<IClient, Client>();
        services.AddSingleton<IOddsClient, OddsClient>();
        services.AddSingleton<IEventProcessor, EventProcessor>();
        services.AddSingleton(ServiceBusClientFactory.CreateServiceBusClient(context.Configuration));
        services.AddSingleton<IKeyVault, KeyVault>();

        services.AddQuartz(q =>
        {
            q.AddJobConfiguration<UpcomingEventsJob>(context.Configuration);
            q.AddJobConfiguration<EventResultsJob>(context.Configuration);
        });

        services.AddQuartzHostedService(q =>
        {
            q.WaitForJobsToComplete = false;
        });
    })
    .Build();

host.Run();

#pragma warning restore CA1852
