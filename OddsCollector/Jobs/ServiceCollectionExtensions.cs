namespace OddsCollector.Jobs;

using Quartz;

internal static class ServiceCollectionExtensions
{
    public static void AddJobConfiguration<T>(
        this IServiceCollectionQuartzConfigurator quartz, IConfiguration config) where T : IJob
    {
        if (quartz is null)
        {
            throw new ArgumentNullException(nameof(quartz));
        }

        if (config is null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        var jobName = typeof(T).Name;
        var configKey = $"Quartz:{jobName}";
        var schedule = config[configKey];

        if (string.IsNullOrEmpty(schedule))
        {
            throw new Exception($"No schedule at {configKey}");
        }

        var jobKey = new JobKey(jobName);
        quartz.AddJob<T>(o => o.WithIdentity(jobKey));
        quartz.AddTrigger(o =>
            o.ForJob(jobKey).WithIdentity($"{jobKey}-trigger").WithCronSchedule(schedule));
    }
}