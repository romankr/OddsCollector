using Microsoft.Extensions.Configuration;
using OddsCollector.Common.Configuration;
using Quartz;

namespace OddsCollector.Common.Scheduler;

public static class ServiceCollectionExtensions
{
    public static void AddJobConfiguration<T>(this IServiceCollectionQuartzConfigurator? quartz,
        IConfiguration? configuration)
        where T : IJob
    {
        if (quartz is null)
        {
            throw new ArgumentNullException(nameof(quartz));
        }

        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var jobName = typeof(T).Name;
        var schedules = configuration.GetRequiredSection<QuartzOptions>().Schedules;
        var schedule = schedules[jobName];

        if (string.IsNullOrEmpty(schedule))
        {
            throw new ConfigurationException($"No schedule for {jobName} job");
        }

        var jobKey = new JobKey(jobName);
        quartz.AddJob<T>(o => o.WithIdentity(jobKey));
        quartz.AddTrigger(o => o.ForJob(jobKey).WithIdentity($"{jobKey}-trigger").WithCronSchedule(schedule));
    }
}
