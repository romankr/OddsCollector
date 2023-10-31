using OddsCollector.Common.Scheduler;
using OddsCollector.Common.ServiceBus;
using OddsCollector.Service.Prediction.Jobs;
using OddsCollector.Service.Prediction.ServiceBus;
using OddsCollector.Service.Prediction.Strategies;
using Quartz;

#pragma warning disable CA1852

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IPredictionStrategy, AdjustedConsensusStrategy>();
        services.AddSingleton(ServiceBusClientFactory.CreateServiceBusClient(context.Configuration));
        services.AddSingleton<IUpcomingEventsProcessor, UpcomingEventsProcessor>();

        services.AddQuartz(q =>
        {
            q.AddJobConfiguration<PredictionJob>(context.Configuration);
        });

        services.AddQuartzHostedService(q =>
        {
            q.WaitForJobsToComplete = false;
        });
    })
    .Build();

host.Run();

#pragma warning restore CA1852
