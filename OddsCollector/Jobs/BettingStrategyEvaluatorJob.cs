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
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _databaseAdapter = databaseAdapter ?? throw new ArgumentNullException(nameof(databaseAdapter));
        _saver = saver ?? throw new ArgumentNullException(nameof(saver));
        _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
        _googleSheetsAdapter = googleSheetsAdapter ?? throw new ArgumentNullException(nameof(googleSheetsAdapter));
    }

    /// <summary>
    /// Evaluates betting strategies and creates reports in Google Sheets and CSV files.
    /// </summary>
    /// <param name="context">An instance of <see cref="IJobExecutionContext"/>.</param>
    /// <returns>An instance of <see cref="Task"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is null.</exception>
    public async Task Execute(IJobExecutionContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

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