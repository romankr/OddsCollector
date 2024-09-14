using Microsoft.Azure.Functions.Worker;

namespace OddsCollector.Functions.Tests.Infrastructure.ServiceBus;

internal static class ServiceBusMessageActionsFactory
{
    public static ServiceBusMessageActions GetServiceBusMessageActionsMock()
    {
        return Substitute.For<ServiceBusMessageActions>();
    }
}
