using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal interface IScoresConverter
{
    IEnumerable<EventResult> ToEventResults(ICollection<Anonymous3>? events, Guid traceId, DateTime timestamp);
}
