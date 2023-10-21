namespace OddsCollector.Common.ServiceBus.Configuration;

public class ServiceBusConfigurationException : Exception
{
    public ServiceBusConfigurationException()
    {
    }

    public ServiceBusConfigurationException(string? message)
        : base(message)
    {
    }

    public ServiceBusConfigurationException(string? message, Exception? inner)
        : base(message, inner)
    {
    }
}
