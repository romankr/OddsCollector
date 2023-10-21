using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using OddsCollector.Common.ServiceBus.Configuration;
using OddsCollector.Common.ServiceBus.Models;

namespace OddsCollector.Service.Notification.Jobs;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed partial class PredictionsProcessor : IPredictionsProcessor
{
    private readonly ServiceBusClient _client;
    private readonly string _eventPredictionsQueue;
    private readonly ILogger<PredictionsProcessor> _logger;

    public PredictionsProcessor(ILogger<PredictionsProcessor>? logger, IConfiguration? configuration,
        ServiceBusClient? client)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _eventPredictionsQueue = ServiceBusConfiguration.GetEventPredictionsQueueName(configuration);
    }

    public async Task StartProcessingAsync()
    {
        var processor = _client.CreateProcessor(_eventPredictionsQueue, new ServiceBusProcessorOptions());

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

        LogPrediction(body);

        await args.CompleteMessageAsync(args.Message).ConfigureAwait(false);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        LogPrediction(args.Exception.ToString());
        return Task.CompletedTask;
    }

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Prediction {Prediction}")]
    public partial void LogPrediction(string prediction);
}
