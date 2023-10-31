using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using OddsCollector.Common.Configuration;
using OddsCollector.Common.ServiceBus;
using OddsCollector.Common.ServiceBus.Configuration;
using OddsCollector.Common.ServiceBus.Models;
using OddsCollector.Service.Prediction.Strategies;

namespace OddsCollector.Service.Prediction.ServiceBus;

internal sealed class UpcomingEventsProcessor : IUpcomingEventsProcessor
{
    private readonly ServiceBusClient _client;
    private readonly string _eventPredictionsQueue;
    private readonly ILogger<UpcomingEventsProcessor> _logger;
    private readonly IPredictionStrategy _strategy;
    private readonly string _upcomingEventsQueue;

    public UpcomingEventsProcessor(ILogger<UpcomingEventsProcessor>? logger, IPredictionStrategy? strategy,
        IConfiguration? configuration, ServiceBusClient? client)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _eventPredictionsQueue = configuration.GetRequiredSection<ServiceBusOptions>().EventPredictionsQueue;
        _upcomingEventsQueue = configuration.GetRequiredSection<ServiceBusOptions>().UpcomingEventsQueue;
    }

    public async Task StartProcessingAsync(CancellationToken token)
    {
        var processor = _client.CreateProcessor(_upcomingEventsQueue, new ServiceBusProcessorOptions());

        try
        {
            processor.ProcessMessageAsync += MessageHandler;

            processor.ProcessErrorAsync += args => DefaultHandlers.ErrorHandler(_logger, args);

            await processor.StartProcessingAsync(token).ConfigureAwait(false);

            await Task.Delay(1000, token).ConfigureAwait(false);

            await processor.StopProcessingAsync(token).ConfigureAwait(false);
        }
        finally
        {
            await processor.DisposeAsync().ConfigureAwait(false);
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();

        var deserialized = JsonConvert.DeserializeObject<UpcomingEvent>(body);

        var prediction = _strategy.GetPrediction(deserialized, DateTime.UtcNow);

        var sender = _client.CreateSender(_eventPredictionsQueue);

        using var messageBatch = await sender.CreateMessageBatchAsync().ConfigureAwait(false);

        var serialized = JsonConvert.SerializeObject(prediction);

        if (!messageBatch.TryAddMessage(new ServiceBusMessage(serialized)))
        {
            _logger.LogError("The message {Body} is too large to fit in the batch.", serialized);
        }

        try
        {
            await sender.SendMessagesAsync(messageBatch).ConfigureAwait(false);
        }
        finally
        {
            await sender.DisposeAsync().ConfigureAwait(false);
        }

        await args.CompleteMessageAsync(args.Message).ConfigureAwait(false);
    }
}
