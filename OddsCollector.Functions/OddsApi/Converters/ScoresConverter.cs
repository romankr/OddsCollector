using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal class ScoresConverter : IScoresConverter
{
    public IEnumerable<EventResult> ToEventResults(ICollection<Anonymous3>? events, Guid traceId, DateTime timestamp)
    {
        throw new NotImplementedException();
    }
}
