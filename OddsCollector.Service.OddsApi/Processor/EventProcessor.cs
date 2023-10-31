using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using OddsCollector.Common.Configuration;
using OddsCollector.Common.ServiceBus.Configuration;
using OddsCollector.Service.OddsApi.Client;

namespace OddsCollector.Service.OddsApi.Processor;

internal sealed class EventProcessor : IEventProcessor
{
    private readonly string _eventResultQueue;
    private readonly IEnumerable<string> _leagues;
    private readonly ILogger<EventProcessor> _logger;
    private readonly IOddsClient _oddsClient;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly string _upcomingEventsQueue;

    public EventProcessor(ILogger<EventProcessor>? logger, IConfiguration? configuration, IOddsClient? oddsClient,
        ServiceBusClient? serviceBusClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _oddsClient = oddsClient ?? throw new ArgumentNullException(nameof(oddsClient));
        _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
        _leagues = configuration.GetRequiredSection<EventProcessorOptions>().Leagues;
        _eventResultQueue = configuration.GetRequiredSection<ServiceBusOptions>().EventResultsQueue;
        _upcomingEventsQueue = configuration.GetRequiredSection<ServiceBusOptions>().UpcomingEventsQueue;
    }

    public async Task GetEventResults(CancellationToken token)
    {
        await ExecuteForEachLeague(_oddsClient.GetEventResultsAsync, _eventResultQueue, token).ConfigureAwait(false);
    }

    public async Task GetUpcomingEvents(CancellationToken token)
    {
        await ExecuteForEachLeague(_oddsClient.GetUpcomingEventsAsync, _upcomingEventsQueue, token)
            .ConfigureAwait(false);
    }

    private async Task ExecuteForEachLeague<T>(Func<string, CancellationToken, Task<IEnumerable<T>>> call,
        string queueName, CancellationToken token)
    {
        foreach (var league in _leagues)
        {
            await Execute(league, call, _serviceBusClient, queueName, _logger, token).ConfigureAwait(false);
        }
    }

    private static async Task Execute<T>(string league, Func<string, CancellationToken, Task<IEnumerable<T>>> call,
        ServiceBusClient serviceBusClient, string queueName, ILogger logger, CancellationToken token)
    {
        var events = await call(league, token).ConfigureAwait(false) ??
                     throw new NotRetrievedException("Failed to retrieve results");

        var sender = serviceBusClient.CreateSender(queueName);

        using var messageBatch = await sender.CreateMessageBatchAsync(token).ConfigureAwait(false);

        foreach (var e in events)
        {
            var serialized = JsonConvert.SerializeObject(e);

            if (!messageBatch.TryAddMessage(new ServiceBusMessage(serialized)))
            {
                logger.LogWarning("The message {Serialized} is too large to fit in the batch.", serialized);
            }
        }

        try
        {
            await sender.SendMessagesAsync(messageBatch, token).ConfigureAwait(false);
        }
        finally
        {
            await sender.DisposeAsync().ConfigureAwait(false);
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
