using System.Runtime.CompilerServices;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Notification.CommunicationServices;
using OddsCollector.Functions.Notification.CosmosDb;

[assembly: InternalsVisibleTo("OddsCollector.Functions.Notification.Tests")]
// DynamicProxyGenAssembly2 is a temporary assembly built by mocking systems that use CastleProxy
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace OddsCollector.Functions.Notification;

internal sealed class NotificationFunction(ILogger<NotificationFunction>? logger, IEmailSender? sender, ICosmosDbClient? client)
{
    private readonly ICosmosDbClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly IEmailSender _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    private readonly ILogger<NotificationFunction> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [Function(nameof(NotificationFunction))]
    public async Task Run([TimerTrigger("%TimerInterval%")] CancellationToken cancellationToken)
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
