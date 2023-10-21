using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace OddsCollector.Service.OddsApi.Jobs;

internal class OddsJobBase
{
    private const string LeaguesKeyName = "OddsApi:Leagues";

    protected static IEnumerable<string> GetLeagues(IConfiguration? configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var leagues = configuration
            .GetSection(LeaguesKeyName)
            .GetChildren()
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrEmpty(v))
            .ToList();

        if (leagues.Count == 0)
        {
            throw new LeaguesNotSpecifiedException($"{LeaguesKeyName} property doesn't have any leagues");
        }

        return leagues!;
    }

    protected static async Task Execute<T>(string league, Func<string, Task<IEnumerable<T>>> call,
        ServiceBusClient serviceBusClient, string queueName, ILogger logger)
    {
        var events = await call(league).ConfigureAwait(false) ??
                     throw new NotRetrievedException("Failed to retrieve results");

        var sender = serviceBusClient.CreateSender(queueName);

        using var messageBatch = await sender.CreateMessageBatchAsync().ConfigureAwait(false);

        foreach (var e in events)
        {
            var serialized = JsonConvert.SerializeObject(e);

            if (!messageBatch.TryAddMessage(new ServiceBusMessage(serialized)))
            {
                logger.LogWarning($"The message {serialized} is too large to fit in the batch.");
            }
        }

        try
        {
            await sender.SendMessagesAsync(messageBatch).ConfigureAwait(false);
        }
        finally
        {
            await sender.DisposeAsync().ConfigureAwait(false);
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
