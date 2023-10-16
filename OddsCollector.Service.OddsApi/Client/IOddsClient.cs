using OddsCollector.Common.ExchangeContracts;

namespace OddsCollector.Service.OddsApi.Client;

internal interface IOddsClient
{
    Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync();
    Task<IEnumerable<EventResult>> GetEventResultsAsync();
}
