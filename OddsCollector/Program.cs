using Microsoft.EntityFrameworkCore;
using OddsCollector.Betting;
using OddsCollector.Betting.Strategies;
using OddsCollector.Csv;
using OddsCollector.DAL;
using OddsCollector.Data;
using OddsCollector.Jobs;
using OddsCollector.OddsApi;
using Quartz;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient<Client>();

        services.AddSingleton<IClient, Client>();
        services.AddSingleton<IOddsApiAdapter, OddsApiAdapter>();
        services.AddScoped<IDatabaseAdapter, DatabaseAdapter>();
        services.AddSingleton<ICsvSaver, CsvSaver>();

        services.AddSingleton<IBettingStrategy, SimpleConsensusStrategy>();
        services.AddSingleton<IBettingStrategy, AdjustedConsensusStrategy>();

        services.AddDbContext<ApplicationDatabaseContext>(
            options => options.UseSqlServer("name=ConnectionStrings:DefaultConnection"));

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