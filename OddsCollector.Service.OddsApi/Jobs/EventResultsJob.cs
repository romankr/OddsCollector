using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using OddsCollector.Service.OddsApi.Client;
using Quartz;

namespace OddsCollector.Service.OddsApi.Jobs;

[DisallowConcurrentExecution]
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed partial class EventResultsJob : IJob
{
    private readonly IOddsClient _client;
    private readonly ILogger<EventResultsJob> _logger;

    public EventResultsJob(ILogger<EventResultsJob>? logger, IOddsClient? client)
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

        var events = await _client.GetEventResultsAsync().ConfigureAwait(false) ??
                     throw new NotRetrievedException("Failed to get event results");

        var jsons = events.Select(JsonConvert.SerializeObject);

        foreach (var json in jsons)
        {
            LogEventResult(json);
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Event Result {Result}")]
    public partial void LogEventResult(string result);
}
