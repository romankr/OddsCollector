namespace OddsCollector.OddsApiService.Jobs;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OddsCollector.OddsApiService.Client;
using Quartz;
using System.Threading.Tasks;

[DisallowConcurrentExecution]
internal class UpcomingEventsJob : IJob
{
    private readonly ILogger<UpcomingEventsJob> _logger;
    private readonly IOddsClient _client;

    public UpcomingEventsJob(ILogger<UpcomingEventsJob>? logger, IOddsClient? client)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task Execute(IJobExecutionContext? context)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));

        var events = await _client.GetUpcomingEventsAsync() ?? throw new NotRetrievedException("Failed to retreive upcoming events");

        var jsons = events.Select(JsonConvert.SerializeObject);

        foreach (var json in jsons) { _logger.LogInformation(json); }

        await Task.CompletedTask;
    }
}