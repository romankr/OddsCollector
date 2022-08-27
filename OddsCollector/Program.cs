using Microsoft.EntityFrameworkCore;
using OddsCollector;
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