using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using OddsCollector.Common.Configuration;
using OddsCollector.Common.ServiceBus.Configuration;

namespace OddsCollector.Common.ServiceBus;

public static class ServiceBusClientFactory
{
    public static ServiceBusClient CreateServiceBusClient(IConfiguration? configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var clientOptions = new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets };

        var name = configuration.GetRequiredSection<ServiceBusOptions>().Name;

        return new ServiceBusClient($"{name}.servicebus.windows.net", new DefaultAzureCredential(), clientOptions);
    }
}
