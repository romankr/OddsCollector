using System.Runtime.CompilerServices;
using Microsoft.Azure.Functions.Worker;
using OddsCollector.Functions.Notification.CommunicationServices;
using OddsCollector.Functions.Notification.CosmosDb;

[assembly: InternalsVisibleTo("OddsCollector.Functions.Notification.Tests")]
// DynamicProxyGenAssembly2 is a temporary assembly built by mocking systems that use CastleProxy
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace OddsCollector.Functions.Notification;

internal sealed class NotificationFunction
{
    private readonly ICosmosDbClient _client;
    private readonly IEmailSender _sender;

    public NotificationFunction(IEmailSender? sender, ICosmosDbClient? client)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    [Function(nameof(NotificationFunction))]
    public async Task Run([TimerTrigger("%TimerInterval%")] TimerInfo timer)
    {
        var predictions = await _client.GetEventPredictionsAsync().ConfigureAwait(false);
        await _sender.SendEmailAsync(predictions).ConfigureAwait(false);
    }
}
