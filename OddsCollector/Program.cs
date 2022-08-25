using Microsoft.EntityFrameworkCore;
using OddsCollector.Api.GoogleApi;
using OddsCollector.Betting;
using OddsCollector.Csv;
using OddsCollector.DAL;
using OddsCollector.Data;
using OddsCollector.Jobs;
using OddsCollector.Api.OddsApi;
using Quartz;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddScoped<IDatabaseAdapter, DatabaseAdapter>();
        services.AddDbContext<ApplicationDatabaseContext>(
            options => options.UseSqlServer("name=ConnectionStrings:DefaultConnection"));

        services.AddHttpClient<Client>();

        services.AddSingleton<IClient, Client>();
        services.AddSingleton<IOddsApiAdapter, OddsApiAdapter>();
        services.AddSingleton<IOddsApiObjectConverter, OddsApiObjectConverter>();
        services.AddSingleton<ICsvSaver, CsvSaver>();
        services.AddSingleton<IGoogleApiAdapter, GoogleApiAdapter>();

        services.AddBettingStrategies();

        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();
            
            q.AddJobConfiguration<OddsCollectorJob>(hostContext.Configuration);
            q.AddJobConfiguration<EventResultCollectorJob>(hostContext.Configuration);
            q.AddJobConfiguration<BettingStrategyEvaluatorJob>(hostContext.Configuration);
        });

        services.AddQuartzHostedService(q => {
            q.WaitForJobsToComplete = true;
        });
    })
    .Build();

await host.RunAsync();