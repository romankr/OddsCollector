namespace OddsCollector.Jobs;

using Csv;
using DAL;
using Betting;
using Quartz;

[DisallowConcurrentExecution]
public class CsvGeneratorJob : IJob
{
    private readonly ILogger<CsvGeneratorJob> _logger;
    private readonly IConfiguration _config;
    private readonly IDatabaseAdapter _databaseAdapter;
    private readonly IBettingStrategy _strategy;
    private readonly ICsvSaver _saver;

    public CsvGeneratorJob(
        ILogger<CsvGeneratorJob> logger, IConfiguration config, IDatabaseAdapter databaseAdapter, IBettingStrategy strategy, ICsvSaver saver)
    {
        _logger = logger;
        _config = config;
        _databaseAdapter = databaseAdapter;
        _strategy = strategy;
        _saver = saver;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Generating CSV-s.");

        try
        {
            var events = _databaseAdapter.GetEventsWithLatestOdds();
            var result = _strategy.Evaluate(events);
            var csvPath = _config.GetValue<string>("CsvOutputPath");
            _saver.WriteBettingStrategyResult(csvPath, _strategy.Name, result);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to generate CSV-s.");
        }

        return Task.CompletedTask;
    }
}