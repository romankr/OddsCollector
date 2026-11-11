using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed partial class OriginalCompletedEventConverter(
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
            switch (ToEventResult(originalEvent))
            {
                case EventResult eventResult:
                    yield return eventResult;
                    break;
                case Rejection rejection:
                    LogEventSkipped(logger, originalEvent.Id, rejection.Reason);
                    break;
            }
        }
    }

    private Conversion<EventResult> ToEventResult(Anonymous3 originalEvent)
    {
        if (string.IsNullOrWhiteSpace(originalEvent.Id))
        {
            return new Rejection("no id");
        }

        if (string.IsNullOrWhiteSpace(originalEvent.Home_team) || string.IsNullOrWhiteSpace(originalEvent.Away_team))
        {
            return new Rejection("no home or away team");
        }

        if (originalEvent.Commence_time.Kind != DateTimeKind.Utc)
        {
            return new Rejection($"commence time is not UTC: {originalEvent.Commence_time.Kind}");
        }

        return converter.GetOutcome(originalEvent.Scores, originalEvent.Home_team, originalEvent.Away_team) switch
        {
            OutcomeType outcome => new EventResult
            {
                Id = originalEvent.Id,
                CommenceTime = originalEvent.Commence_time,
                Outcome = outcome
            },
            Rejection rejection => rejection
        };
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Skipped event {Id}: {Reason}")]
    private static partial void LogEventSkipped(ILogger logger, string? id, string reason);
}
