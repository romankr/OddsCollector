using Microsoft.Extensions.Configuration;

namespace OddsCollector.Common.ServiceBus.Configuration;

public static class ServiceBusConfiguration
{
    private const string ServiceBusNameKey = "ServiceBus:Name";
    private const string EventResultsQueueKey = "ServiceBus:EventResultsQueue";
    private const string UpcomingEventsQueueKey = "ServiceBus:UpcomingEventsQueue";
    private const string EventPredictionsQueueKey = "ServiceBus:EventPredictionsQueue";

    public static string GetServiceBusName(IConfiguration? configuration)
    {
        return GetKey(configuration, ServiceBusNameKey);
    }

    public static string GetEventResultsQueueName(IConfiguration? configuration)
    {
        return GetKey(configuration, EventResultsQueueKey);
    }

    public static string GetUpcomingEventsQueueName(IConfiguration? configuration)
    {
        return GetKey(configuration, UpcomingEventsQueueKey);
    }

    public static string GetEventPredictionsQueueName(IConfiguration? configuration)
    {
        return GetKey(configuration, EventPredictionsQueueKey);
    }

    private static string GetKey(IConfiguration? configuration, string key)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var value = configuration[key];

        if (string.IsNullOrEmpty(value))
        {
            throw new ServiceBusConfigurationException($"{key} property is null or empty.");
        }

        return value;
    }
}
