using OddsCollector.Common.Scheduler;
using OddsCollector.Common.ServiceBus;
using OddsCollector.Service.Notification.Jobs;
using Quartz;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton(ServiceBusCreator.GetServiceBusClient(context.Configuration));
        services.AddSingleton<IPredictionsProcessor, PredictionsProcessor>();

        services.AddQuartz(q =>
        {
            q.AddJobConfiguration<NotificationJob>(context.Configuration);
        });

        services.AddQuartzHostedService(q =>
        {
            q.WaitForJobsToComplete = true;
        });
    })
    .Build();

host.Run();
