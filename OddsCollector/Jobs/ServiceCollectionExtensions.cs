namespace OddsCollector.Jobs;

using Quartz;

/// <summary>
/// Extension methods for the dependency injection.
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a job in <see cref="IServiceCollectionQuartzConfigurator"/> and reads the job schedule from <see cref="IConfiguration"/>.
    /// </summary>
    /// <typeparam name="T">A type that implements <see cref="IJob"/>.</typeparam>
    /// <param name="quartz">An instance of <see cref="IServiceCollectionQuartzConfigurator"/> where to register the job.</param>
    /// <param name="config">An instance of <see cref="IConfiguration"/> to read the job schedule from.</param>
    /// <exception cref="ArgumentNullException">Either <paramref name="quartz"/> or <paramref name="config"/> are null.</exception>
    /// <exception cref="Exception">No schedule available for the provided instance of <see cref="IJob"/>.</exception>
    public static void AddJobConfiguration<T>(
        this IServiceCollectionQuartzConfigurator quartz, IConfiguration config) 
        where T : IJob
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
        var configKey = $"Quartz:{jobName}.Schedule";
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