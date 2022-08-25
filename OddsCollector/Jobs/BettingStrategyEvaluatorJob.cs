namespace OddsCollector.Jobs;

using Api.GoogleApi;
using Betting;
using Common;
using Csv;
using DAL;
using Quartz;

/// <summary>
/// Evaluates betting strategies and creates reports in Google Sheets and CSV files.
/// </summary>
[DisallowConcurrentExecution]
public class BettingStrategyEvaluatorJob : IJob
{
    private readonly ILogger<BettingStrategyEvaluatorJob> _logger;
    private readonly IConfiguration _config;
    private readonly IDatabaseAdapter _databaseAdapter;
    private readonly IEnumerable<IBettingStrategy> _strategies;
    private readonly ICsvSaver _saver;
    private readonly IGoogleApiAdapter _googleSheetsAdapter;

    /// <summary>
    /// A constructor that is suitable for the dependency injection.
    /// </summary>
    /// <param name="logger">An instance of <see cref="ILogger"/>.</param>
    /// <param name="config">An instance of <see cref="IConfiguration"/>.</param>
    /// <param name="databaseAdapter">An instance of <see cref="IDatabaseAdapter"/>.</param>
    /// <param name="strategies">An instance of <see cref="IEnumerable{IBettingStrategy}"/>.</param>
    /// <param name="saver">An instance of <see cref="ICsvSaver"/>.</param>
    /// <param name="googleSheetsAdapter">An instance of <see cref="IGoogleApiAdapter"/>.</param>
    public BettingStrategyEvaluatorJob(
        ILogger<BettingStrategyEvaluatorJob> logger, 
        IConfiguration config, 
        IDatabaseAdapter databaseAdapter, 
        IEnumerable<IBettingStrategy> strategies, 
        ICsvSaver saver,
        IGoogleApiAdapter googleSheetsAdapter)
    {
        ArgumentChecker.NullCheck(logger, nameof(logger));
        ArgumentChecker.NullCheck(config, nameof(config));
        ArgumentChecker.NullCheck(databaseAdapter, nameof(databaseAdapter));
        ArgumentChecker.NullCheck(saver, nameof(saver));
        ArgumentChecker.NullCheck(strategies, nameof(strategies));
        ArgumentChecker.NullCheck(googleSheetsAdapter, nameof(googleSheetsAdapter));

        _logger = logger;
        _config = config;
        _databaseAdapter = databaseAdapter;
        _saver = saver;
        _strategies = strategies;
        _googleSheetsAdapter = googleSheetsAdapter;
    }

    /// <summary>
    /// Evaluates betting strategies and creates reports in Google Sheets and CSV files.
    /// </summary>
    /// <param name="context">An instance of <see cref="IJobExecutionContext"/>.</param>
    /// <returns>An instance of <see cref="Task"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is null.</exception>
    public async Task Execute(IJobExecutionContext context)
    {
        ArgumentChecker.NullCheck(context, nameof(context));

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