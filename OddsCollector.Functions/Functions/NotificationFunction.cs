using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.CommunicationServices;
using OddsCollector.Functions.CosmosDb;

namespace OddsCollector.Functions.Functions;

internal sealed class NotificationFunction(ILogger<NotificationFunction>? logger, IEmailSender? sender, ICosmosDbClient? client)
{
    private readonly ICosmosDbClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly IEmailSender _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    private readonly ILogger<NotificationFunction> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [Function(nameof(NotificationFunction))]
    public async Task Run([TimerTrigger("%NotificationFunction:TimerInterval%")] CancellationToken cancellationToken)
    {
        try
        {
            var predictions = await _client.GetEventPredictionsAsync(cancellationToken).ConfigureAwait(false);
            await _sender.SendEmailAsync(predictions, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to send e-mail notification");
        }
    }
}
