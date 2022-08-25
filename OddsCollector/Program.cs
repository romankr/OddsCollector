using Microsoft.EntityFrameworkCore;
using OddsCollector.Csv;
using OddsCollector.DAL;
using OddsCollector.Data;
using OddsCollector.Jobs;
using OddsCollector.OddsApi;
using OddsCollector.Prediction;
using Quartz;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient<Client>();

        services.AddSingleton<IClient, Client>();
        services.AddSingleton<IOddsApiAdapter, OddsApiAdapter>();
        services.AddScoped<IDatabaseAdapter, DatabaseAdapter>();
        services.AddSingleton<IPredictor, Predictor>();
        services.AddSingleton<ICsvSaver, CsvSaver>();

        services.AddDbContext<ApplicationDatabaseContext>(
            options => options.UseSqlServer("name=ConnectionStrings:DefaultConnection"));

        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();
            
            q.AddJobConfiguration<OddsCollectorJob>(hostContext.Configuration);
            q.AddJobConfiguration<ResultCollectorJob>(hostContext.Configuration);
            q.AddJobConfiguration<CsvGeneratorJob>(hostContext.Configuration);
        });

        services.AddQuartzHostedService(q => {
            q.WaitForJobsToComplete = true;
        });
    })
    .Build();

await host.RunAsync();

public static class ServiceCollectionExtensions
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
