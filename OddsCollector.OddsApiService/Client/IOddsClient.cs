namespace OddsCollector.OddsApiService.Client;

using OddsCollector.Common.ExchangeContracts;

internal interface IOddsClient
{
    Task<IEnumerable<UpcomingEvent>> GetUpcomingEventsAsync();
    Task<IEnumerable<EventResult>> GetEventResultsAsync();
}