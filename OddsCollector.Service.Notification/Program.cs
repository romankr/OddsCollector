using OddsCollector.Common.Scheduler;
using OddsCollector.Service.Notification.Jobs;
using Quartz;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
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
