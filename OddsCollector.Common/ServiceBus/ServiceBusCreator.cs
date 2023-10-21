using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using OddsCollector.Common.ServiceBus.Configuration;

namespace OddsCollector.Common.ServiceBus;

public static class ServiceBusCreator
{
    public static ServiceBusClient GetServiceBusClient(IConfiguration? configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var clientOptions = new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets };

        var name = ServiceBusConfiguration.GetServiceBusName(configuration);

        return new ServiceBusClient($"{name}.servicebus.windows.net", new DefaultAzureCredential(), clientOptions);
    }
}
