namespace OddsCollector.Jobs;

using Common;
using DAL;
using Api.OddsApi;
using Quartz;
using System.Threading.Tasks;

[DisallowConcurrentExecution]
public class OddsCollectorJob : IJob
{
    private readonly ILogger<OddsCollectorJob> _logger;
    private readonly IConfiguration _config;
    private readonly IOddsApiAdapter _apiAdapter;
    private readonly IDatabaseAdapter _databaseAdapter;

    public OddsCollectorJob(
        ILogger<OddsCollectorJob> logger, IConfiguration config, IOddsApiAdapter apiAdapter, IDatabaseAdapter databaseAdapter)
    {
        _logger = logger;
        _config = config;
        _apiAdapter = apiAdapter;
        _databaseAdapter = databaseAdapter;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Collecting odds.");
            
        try
        {
            var leagues = ConfigurationReader.GetLeagues(_config);
            var events = _apiAdapter.GetUpcomingEventsAsync(leagues).GetAwaiter().GetResult();
            _databaseAdapter.SaveUpcomingEvents(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect odds.");
        }
            
        return Task.CompletedTask;
    }
}