namespace OddsCollector.Jobs;

using Api.GoogleApi;
using Betting;
using Common;
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

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Evaluating strategies.");

        try
        {
            var generateCsv = ConfigurationReader.GetGenerateCsv(_config);
            var generateGoogleSheets = ConfigurationReader.GetGenerateGoogleSheets(_config);

            if (!generateCsv && !generateGoogleSheets)
            {
                await Task.CompletedTask;
            }

            var events = _databaseAdapter.GetEventsWithLatestOdds().ToList();

            var tasks = _strategies.Select(async s =>
            {
                var result = s.Evaluate(events);

                if (generateCsv)
                {
                    await _saver.WriteBettingStrategyResultAsync(s.GetType().Name, result);
                }

                if (generateGoogleSheets)
                {
                    await _googleSheetsAdapter.CreateReportAsync(s.GetType().Name, result);
                }
            });

            await Task.WhenAll(tasks);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate strategies.");
        }

        await Task.CompletedTask;
    }
}