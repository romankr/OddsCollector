namespace OddsCollector.OddsApiService.Client;

using OddsCollector.Common.ExchangeContracts;

internal interface IConverter
{
    IEnumerable<UpcomingEvent> ToUpcomingEvents(ICollection<Anonymous2>[]? events, Guid? traceId, DateTime? timestamp);
    IEnumerable<EventResult> ToCompletedEvents(ICollection<Anonymous3>[]? events, Guid? traceId, DateTime? timestamp);
}