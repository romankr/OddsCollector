using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converter;

public interface IOddsApiObjectConverter
{
    IEnumerable<UpcomingEvent> ToUpcomingEvents(ICollection<Anonymous2>? events, Guid traceId, DateTime timestamp);
    IEnumerable<EventResult> ToEventResults(ICollection<Anonymous3>? events, Guid traceId, DateTime timestamp);
}
