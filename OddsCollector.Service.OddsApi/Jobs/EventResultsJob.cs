using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus;
using OddsCollector.Common.ServiceBus.Configuration;
using OddsCollector.Service.OddsApi.Client;
using Quartz;

namespace OddsCollector.Service.OddsApi.Jobs;

[DisallowConcurrentExecution]
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class EventResultsJob : OddsJobBase, IJob
{
    private readonly IEnumerable<string> _leagues;
    private readonly ILogger<EventResultsJob> _logger;
    private readonly IOddsClient _oddsClient;
    private readonly string _queueName;
    private readonly ServiceBusClient _serviceBusClient;

    public EventResultsJob(ILogger<EventResultsJob>? logger, IConfiguration? configuration, IOddsClient? oddsClient,
        ServiceBusClient? serviceBusClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _oddsClient = oddsClient ?? throw new ArgumentNullException(nameof(oddsClient));
        _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
        _queueName = ServiceBusConfiguration.GetEventResultsQueueName(configuration);
        _leagues = GetLeagues(configuration);
    }

    public async Task Execute(IJobExecutionContext? context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        foreach (var league in _leagues)
        {
            await Execute(league, _oddsClient.GetEventResultsAsync, _serviceBusClient, _queueName, _logger)
                .ConfigureAwait(false);
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
