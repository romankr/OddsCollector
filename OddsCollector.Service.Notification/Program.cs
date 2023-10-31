using OddsCollector.Common.Scheduler;
using OddsCollector.Common.ServiceBus;
using OddsCollector.Service.Notification.Email;
using OddsCollector.Service.Notification.Jobs;
using OddsCollector.Service.Notification.ServiceBus;
using Quartz;

#pragma warning disable CA1852

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton(ServiceBusClientFactory.CreateServiceBusClient(context.Configuration));
        services.AddSingleton<IPredictionsProcessor, PredictionsProcessor>();
        services.AddSingleton<IEmailSender, EmailSender>();

        services.AddQuartz(q =>
        {
            q.AddJobConfiguration<ServiceBusReaderJob>(context.Configuration);
            q.AddJobConfiguration<EmailSenderJob>(context.Configuration);
        });

        services.AddQuartzHostedService(q =>
        {
            q.WaitForJobsToComplete = false;
        });
    })
    .Build();

host.Run();

#pragma warning restore CA1852
