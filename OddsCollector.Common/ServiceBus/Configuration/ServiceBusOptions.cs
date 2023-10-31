namespace OddsCollector.Common.ServiceBus.Configuration;

public class ServiceBusOptions
{
    public string Name { get; set; } = string.Empty;

    public string EventResultsQueue { get; set; } = string.Empty;

    public string UpcomingEventsQueue { get; set; } = string.Empty;

    public string EventPredictionsQueue { get; set; } = string.Empty;
}
