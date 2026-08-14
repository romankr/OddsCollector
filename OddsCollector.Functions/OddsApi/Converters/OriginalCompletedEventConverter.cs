using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class OriginalCompletedEventConverter(IOutcomeConverter converter) : IOriginalCompletedEventConverter
{
    public IEnumerable<EventResult> ToEventResults(ICollection<Anonymous3>? originalEvents)
    {
        CheckParameters(originalEvents);

        foreach (var originalEvent in originalEvents!)
        {
            yield return ToEventResult(originalEvent);
        }
    }

    private EventResult ToEventResult(Anonymous3 originalEvent)
    {
        return new EventResultBuilder()
            .SetId(originalEvent.Id)
            .SetCommenceTime(originalEvent.Commence_time)
            .SetOutcome(converter.GetOutcome(originalEvent.Scores))
            .Instance;
    }

    private static void CheckParameters(ICollection<Anonymous3>? originalEvents)
    {
        ArgumentNullException.ThrowIfNull(originalEvents);
    }
}
