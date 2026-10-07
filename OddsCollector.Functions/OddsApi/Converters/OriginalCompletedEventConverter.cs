using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class OriginalCompletedEventConverter(
    ILogger<OriginalCompletedEventConverter> logger,
    IOutcomeConverter converter) : IOriginalCompletedEventConverter
{
    public IEnumerable<EventResult> ToEventResults(ICollection<Anonymous3>? originalEvents)
    {
        ArgumentNullException.ThrowIfNull(originalEvents);

        return Iterate(originalEvents);
    }

    private IEnumerable<EventResult> Iterate(ICollection<Anonymous3> originalEvents)
    {
        foreach (var originalEvent in originalEvents.Where(e => e.Completed == true))
        {
            var eventResult = TryToEventResult(originalEvent);

            if (eventResult is not null)
            {
                yield return eventResult;
            }
        }
    }

    private EventResult? TryToEventResult(Anonymous3 originalEvent)
    {
        try
        {
            return ToEventResult(originalEvent);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            logger.LogWarning(exception, "Skipped event {Id}", originalEvent.Id);

            return null;
        }
    }

    private EventResult ToEventResult(Anonymous3 originalEvent)
    {
        // A missing id is rejected by EventResult.
        return new EventResult
        {
            Id = originalEvent.Id ?? string.Empty,
            CommenceTime = originalEvent.Commence_time,
            Outcome = converter.GetOutcome(originalEvent.Scores)
        };
    }
}
