using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class OriginalUpcomingEventConverter(
    ILogger<OriginalUpcomingEventConverter> logger,
    IBookmakerConverter bookmakerConverter)
    : IOriginalUpcomingEventConverter
{
    public IEnumerable<UpcomingEvent> ToUpcomingEvents(ICollection<Anonymous2>? events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return Iterate(events);
    }

    private IEnumerable<UpcomingEvent> Iterate(ICollection<Anonymous2> events)
    {
        foreach (var originalEvent in events)
        {
            var upcomingEvent = TryToUpcomingEvent(originalEvent);

            if (upcomingEvent is not null)
            {
                yield return upcomingEvent;
            }
        }
    }

    // One malformed event must not discard the rest of its league.
    private UpcomingEvent? TryToUpcomingEvent(Anonymous2 originalEvent)
    {
        try
        {
            var upcomingEvent = ToUpcomingEvent(originalEvent);

            // Without a usable quote there is nothing to predict, and a prediction could
            // only fail on it, so the event is left out rather than sent on.
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
        return new UpcomingEventBuilder()
            .SetAwayTeam(originalEvent.Away_team)
            .SetHomeTeam(originalEvent.Home_team)
            .SetId(originalEvent.Id)
            .SetCommenceTime(originalEvent.Commence_time)
            .SetOdds(
                bookmakerConverter.ToOdds(originalEvent.Bookmakers, originalEvent.Away_team, originalEvent.Home_team)
                    .ToList()
            )
            .Instance;
    }
}
