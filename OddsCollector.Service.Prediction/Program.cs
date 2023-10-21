using OddsCollector.Common.Scheduler;
using OddsCollector.Common.ServiceBus;
using OddsCollector.Service.Prediction.Jobs;
using OddsCollector.Service.Prediction.Strategies;
using Quartz;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IPredictionStrategy, AdjustedConsensusStrategy>();
        services.AddSingleton(ServiceBusCreator.GetServiceBusClient(context.Configuration));
        services.AddSingleton<IUpcomingEventsProcessor, UpcomingEventsProcessor>();

        services.AddQuartz(q =>
        {
            q.AddJobConfiguration<PredictionJob>(context.Configuration);
        });

        services.AddQuartzHostedService(q =>
        {
            q.WaitForJobsToComplete = true;
        });
    })
    .Build();

host.Run();
