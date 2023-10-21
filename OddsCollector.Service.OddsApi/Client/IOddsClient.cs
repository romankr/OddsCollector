using OddsCollector.Common.ServiceBus.Models;

namespace OddsCollector.Service.OddsApi.Client;

internal interface IOddsClient
{
    Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync(string league);
    Task<IEnumerable<EventResult>> GetEventResultsAsync(string league);
}
