using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converter;

internal interface IOddsApiObjectConverter
{
    IEnumerable<UpcomingEvent> ToUpcomingEvents(ICollection<Anonymous2>? events);
    IEnumerable<EventResult> ToEventResults(ICollection<Anonymous3>? events);
}
