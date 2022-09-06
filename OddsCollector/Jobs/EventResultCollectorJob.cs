namespace OddsCollector.Jobs;

using Api.OddsApi;
using Common;
using DAL;
using Quartz;
using System.Threading.Tasks;

/// <summary>
/// Downloads event results from the Odds API.
/// </summary>
[DisallowConcurrentExecution]
public class EventResultCollectorJob : IJob
{
    private readonly ILogger<EventResultCollectorJob> _logger;
    private readonly IConfiguration _config;
    private readonly IOddsApiAdapter _apiAdapter;
    private readonly IDatabaseAdapter _databaseAdapter;

    /// <summary>
    /// A constructor that is suitable for the dependency injection.
    /// </summary>
    /// <param name="logger">An instance of <see cref="ILogger{EventResultCollectorJob}"/>.</param>
    /// <param name="config">An instance of <see cref="IConfiguration"/>.</param>
    /// <param name="apiAdapter">An instance of <see cref="IOddsApiAdapter"/>.</param>
    /// <param name="databaseAdapter">An instance of <see cref="IDatabaseAdapter"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// Either <paramref name="logger"/> or <paramref name="config"/> or
    /// <paramref name="apiAdapter"/> or <paramref name="databaseAdapter"/> are null.
    /// </exception>
    public EventResultCollectorJob(
        ILogger<EventResultCollectorJob> logger, IConfiguration config, IOddsApiAdapter apiAdapter, IDatabaseAdapter databaseAdapter)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _apiAdapter = apiAdapter ?? throw new ArgumentNullException(nameof(apiAdapter));
        _databaseAdapter = databaseAdapter ?? throw new ArgumentNullException(nameof(databaseAdapter));
    }

    /// <summary>
    /// Downloads event results from the Odds API.
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

        _logger.LogInformation("Collecting results.");

        try
        {
            var leagues = ConfigurationReader.GetLeagues(_config).ToList();

            if (leagues.Count == 0)
            {
                throw new Exception("No leagues found.");
            }

            var events = await _apiAdapter.GetCompletedEventsAsync(leagues);
            await _databaseAdapter.SaveEventResultsAsync(events);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to collect results.");
        }

        await Task.CompletedTask;
    }
}