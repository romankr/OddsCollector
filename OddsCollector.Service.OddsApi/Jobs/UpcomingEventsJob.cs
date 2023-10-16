using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using OddsCollector.Service.OddsApi.Client;
using Quartz;

namespace OddsCollector.Service.OddsApi.Jobs;

[DisallowConcurrentExecution]
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed partial class UpcomingEventsJob : IJob
{
    private readonly IOddsClient _client;
    private readonly ILogger<UpcomingEventsJob> _logger;

    public UpcomingEventsJob(ILogger<UpcomingEventsJob>? logger, IOddsClient? client)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task Execute(IJobExecutionContext? context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var events = await _client.GetUpcomingEventsAsync().ConfigureAwait(false) ??
                     throw new NotRetrievedException("Failed to get upcoming events");

        var jsons = events.Select(JsonConvert.SerializeObject);

        foreach (var json in jsons)
        {
            LogUpcomingEvent(json);
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Upcoming Event {UpcomingEvent}")]
    public partial void LogUpcomingEvent(string upcomingEvent);
}
