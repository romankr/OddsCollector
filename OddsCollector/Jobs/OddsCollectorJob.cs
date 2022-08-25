namespace OddsCollector.Jobs;

using Api.OddsApi;
using Common;
using DAL;
using Quartz;
using System.Threading.Tasks;

/// <summary>
/// A job that simply downloads odds from the Odds API.
/// </summary>
[DisallowConcurrentExecution]
public class OddsCollectorJob : IJob
{
    private readonly ILogger<OddsCollectorJob> _logger;
    private readonly IConfiguration _config;
    private readonly IOddsApiAdapter _apiAdapter;
    private readonly IDatabaseAdapter _databaseAdapter;

    /// <summary>
    /// A constructor that is suitable for the dependency injection.
    /// </summary>
    /// <param name="logger">An instance of <see cref="ILogger{OddsCollectorJob}"/>.</param>
    /// <param name="config">An instance of <see cref="IConfiguration"/>.</param>
    /// <param name="apiAdapter">An instance of <see cref="IOddsApiAdapter"/>.</param>
    /// <param name="databaseAdapter">An instance of <see cref="IDatabaseAdapter"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// Either <paramref name="logger"/> or <paramref name="config"/> or
    /// <paramref name="apiAdapter"/> or <paramref name="databaseAdapter"/> are null.
    /// </exception>
    public OddsCollectorJob(
        ILogger<OddsCollectorJob> logger, IConfiguration config, IOddsApiAdapter apiAdapter, IDatabaseAdapter databaseAdapter)
    {
        ArgumentChecker.NullCheck(logger, nameof(logger));
        ArgumentChecker.NullCheck(config, nameof(config));
        ArgumentChecker.NullCheck(apiAdapter, nameof(apiAdapter));
        ArgumentChecker.NullCheck(databaseAdapter, nameof(databaseAdapter));

        _logger = logger;
        _config = config;
        _apiAdapter = apiAdapter;
        _databaseAdapter = databaseAdapter;
    }

    /// <summary>
    /// Downloads odds form the Odds API.
    /// </summary>
    /// <param name="context">An instance of <see cref="IJobExecutionContext"/>.</param>
    /// <returns>An instance of <see cref="Task"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is null.</exception>
    public async Task Execute(IJobExecutionContext context)
    {
        ArgumentChecker.NullCheck(context, nameof(context));

        _logger.LogInformation("Collecting odds.");
         
        try
        {
            var leagues = ConfigurationReader.GetLeagues(_config).ToList();

            if (leagues.Count == 0)
            {
                throw new Exception("No leagues found.");
            }

            var events = await _apiAdapter.GetUpcomingEventsAsync(leagues);
            await _databaseAdapter.SaveUpcomingEventsAsync(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect odds.");
        }

        await Task.CompletedTask;
    }
}