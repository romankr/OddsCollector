using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class OriginalCompletedEventConverter(IWinnerConverter converter) : IOriginalCompletedEventConverter
{
    public IEnumerable<EventResult> ToEventResults(ICollection<Anonymous3>? originalEvents)
    {
        ArgumentNullException.ThrowIfNull(originalEvents);

        foreach (var originalEvent in originalEvents)
        {
            yield return ToEventResult(originalEvent);
        }
    }

    private EventResult ToEventResult(Anonymous3 originalEvent)
    {
        return new EventResultBuilder()
            .SetId(originalEvent.Id)
            .SetCommenceTime(originalEvent.Commence_time)
            .SetWinner(converter.GetWinner(originalEvent.Scores))
            .Instance;
    }
}
