using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal interface IOriginalCompletedEventConverter
{
    IEnumerable<EventResult> ToEventResults(ICollection<Anonymous3>? originalEvents);
}
