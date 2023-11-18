using OddsCollector.Common.Models;
using OddsCollector.Common.OddsApi.WebApi;

namespace OddsCollector.Common.OddsApi.Converter;

public interface IOddsApiObjectConverter
{
    IEnumerable<UpcomingEvent> ToUpcomingEvents(ICollection<Anonymous2>? events, Guid traceId, DateTime timestamp);
    IEnumerable<EventResult> ToEventResults(ICollection<Anonymous3>? events, Guid traceId, DateTime timestamp);
}
