using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class OriginalUpcomingEventConverter(
    ILogger<OriginalUpcomingEventConverter> logger,
    IBookmakerConverter bookmakerConverter,
    TimeProvider timeProvider)
    : IOriginalUpcomingEventConverter
{
    public IEnumerable<UpcomingEvent> ToUpcomingEvents(ICollection<Anonymous2>? events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return Iterate(events);
    }

    private IEnumerable<UpcomingEvent> Iterate(ICollection<Anonymous2> events)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var originalEvent in events)
        {
            // The odds endpoint also returns games in play. Their odds already reflect the
            // score, and a prediction made from them would overwrite the pre-match one.
            // A time that is not UTC cannot be compared with now. The builder rejects it with a warning,
            // where skipping it here as started would only log at debug level.
            if (originalEvent.Commence_time.Kind == DateTimeKind.Utc && originalEvent.Commence_time <= now)
            {
                logger.LogDebug("Skipped event {Id}: already started", originalEvent.Id);

                continue;
            }

            var upcomingEvent = TryToUpcomingEvent(originalEvent);

            if (upcomingEvent is not null)
            {
                yield return upcomingEvent;
            }
        }
    }

    private UpcomingEvent? TryToUpcomingEvent(Anonymous2 originalEvent)
    {
        try
        {
            var upcomingEvent = ToUpcomingEvent(originalEvent);

            if (!upcomingEvent.Odds.Any())
            {
                logger.LogWarning("Skipped event {Id}: no usable odds", originalEvent.Id);

                return null;
            }

            return upcomingEvent;
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            logger.LogWarning(exception, "Skipped event {Id}", originalEvent.Id);

            return null;
        }
    }

    private UpcomingEvent ToUpcomingEvent(Anonymous2 originalEvent)
    {
        // Missing values are rejected by UpcomingEvent, before the odds are converted.
        return new UpcomingEvent
        {
            AwayTeam = originalEvent.Away_team ?? string.Empty,
            HomeTeam = originalEvent.Home_team ?? string.Empty,
            Id = originalEvent.Id ?? string.Empty,
            CommenceTime = originalEvent.Commence_time,
            Odds =
            [
                .. bookmakerConverter.ToOdds(originalEvent.Bookmakers, originalEvent.Away_team,
                    originalEvent.Home_team)
            ]
        };
    }
}
