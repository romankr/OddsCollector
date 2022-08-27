namespace OddsCollector.Jobs;

using Common;
using DAL;
using OddsApi;
using Quartz;
using System.Threading.Tasks;

[DisallowConcurrentExecution]
public class ResultCollectorJob : IJob
{
    private readonly ILogger<ResultCollectorJob> _logger;
    private readonly IConfiguration _config;
    private readonly IOddsApiAdapter _apiAdapter;
    private readonly IDatabaseAdapter _databaseAdapter;

    public ResultCollectorJob(
        ILogger<ResultCollectorJob> logger, IConfiguration config, IOddsApiAdapter apiAdapter, IDatabaseAdapter databaseAdapter)
    {
        _logger = logger;
        _config = config;
        _apiAdapter = apiAdapter;
        _databaseAdapter = databaseAdapter;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Collecting results.");

        try
        {
            var leagues = ConfigurationReader.GetLeagues(_config);
            var events = _apiAdapter.GetCompletedEventsAsync(leagues).GetAwaiter().GetResult();
            _databaseAdapter.SaveEventResults(events);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to collect results.");
        }

        return Task.CompletedTask;
    }
}