using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using OddsCollector.Common.Configuration;
using OddsCollector.Common.ServiceBus;
using OddsCollector.Common.ServiceBus.Configuration;
using OddsCollector.Common.ServiceBus.Models;
using OddsCollector.Service.Notification.Email;

namespace OddsCollector.Service.Notification.ServiceBus;

internal sealed class PredictionsProcessor : IPredictionsProcessor
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<PredictionsProcessor> _logger;
    private readonly string _queue;
    private readonly IEmailSender _sender;

    public PredictionsProcessor(ILogger<PredictionsProcessor>? logger, IConfiguration? configuration,
        ServiceBusClient? client, IEmailSender sender)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _queue = configuration.GetRequiredSection<ServiceBusOptions>().EventPredictionsQueue;
    }

    public async Task StartProcessingAsync(CancellationToken token)
    {
        var processor = _client.CreateProcessor(_queue, new ServiceBusProcessorOptions());

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

        var deserialized = JsonConvert.DeserializeObject<EventPrediction>(body);

        _sender.Add(deserialized);

        await args.CompleteMessageAsync(args.Message).ConfigureAwait(false);
    }
}
