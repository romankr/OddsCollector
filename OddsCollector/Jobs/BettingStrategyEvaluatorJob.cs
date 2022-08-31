using OddsCollector.Api.GoogleApi;

namespace OddsCollector.Jobs;

using Betting;
using Csv;
using DAL;
using Quartz;

[DisallowConcurrentExecution]
public class BettingStrategyEvaluatorJob : IJob
{
    private readonly ILogger<BettingStrategyEvaluatorJob> _logger;
    private readonly IConfiguration _config;
    private readonly IDatabaseAdapter _databaseAdapter;
    private readonly IEnumerable<IBettingStrategy> _strategies;
    private readonly ICsvSaver _saver;
    private readonly IGoogleApiAdapter _googleSheetsAdapter;

    public BettingStrategyEvaluatorJob(
        ILogger<BettingStrategyEvaluatorJob> logger, 
        IConfiguration config, 
        IDatabaseAdapter databaseAdapter, 
        IEnumerable<IBettingStrategy> strategies, 
        ICsvSaver saver,
        IGoogleApiAdapter googleSheetsAdapter)
    {
        _logger = logger;
        _config = config;
        _databaseAdapter = databaseAdapter;
        _saver = saver;
        _strategies = strategies;
        _googleSheetsAdapter = googleSheetsAdapter;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Evaluating strategies.");

        try
        {
            var csvPath = _config.GetValue<string>("CsvOutputPath");
            var events = _databaseAdapter.GetEventsWithLatestOdds().ToList();

            foreach (var strategy in _strategies)
            {
                var result = strategy.Evaluate(events);
                _saver.WriteBettingStrategyResult(csvPath, strategy.GetType().Name, result);
                _googleSheetsAdapter.CreateReport(strategy.GetType().Name, result);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate strategies.");
        }

        return Task.CompletedTask;
    }
}