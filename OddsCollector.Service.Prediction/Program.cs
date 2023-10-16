using OddsCollector.Common.Scheduler;
using OddsCollector.Service.Prediction.Jobs;
using OddsCollector.Service.Prediction.Strategies;
using Quartz;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IPredictionStrategy, AdjustedConsensusStrategy>();

        services.AddQuartz(q =>
        {
            q.AddJobConfiguration<EventResultsJob>(context.Configuration);
        });

        services.AddQuartzHostedService(q =>
        {
            q.WaitForJobsToComplete = true;
        });
    })
    .Build();

host.Run();
