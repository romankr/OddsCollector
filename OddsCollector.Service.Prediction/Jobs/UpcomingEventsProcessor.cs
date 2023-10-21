using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using OddsCollector.Common.ServiceBus.Configuration;
using OddsCollector.Common.ServiceBus.Models;
using OddsCollector.Service.Prediction.Strategies;

namespace OddsCollector.Service.Prediction.Jobs;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed partial class UpcomingEventsProcessor : IUpcomingEventsProcessor
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
        _eventPredictionsQueue = ServiceBusConfiguration.GetEventPredictionsQueueName(configuration);
        _upcomingEventsQueue = ServiceBusConfiguration.GetUpcomingEventsQueueName(configuration);
    }

    public async Task StartProcessingAsync()
    {
        var processor = _client.CreateProcessor(_upcomingEventsQueue, new ServiceBusProcessorOptions());

        try
        {
            processor.ProcessMessageAsync += MessageHandler;

            processor.ProcessErrorAsync += ErrorHandler;

            await processor.StartProcessingAsync().ConfigureAwait(false);

            await Task.Delay(1000).ConfigureAwait(false);

            await processor.StopProcessingAsync().ConfigureAwait(false);
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
            LogError($"The message {serialized} is too large to fit in the batch.");
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

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        LogError(args.Exception.ToString());
        return Task.CompletedTask;
    }

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Error {Result}")]
    public partial void LogError(string result);
}
