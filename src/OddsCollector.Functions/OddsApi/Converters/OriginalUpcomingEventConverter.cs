using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed partial class OriginalUpcomingEventConverter(
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
            if (originalEvent.Commence_time.Kind == DateTimeKind.Utc && originalEvent.Commence_time <= now)
            {
                LogStartedEventSkipped(logger, originalEvent.Id);

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

            if (upcomingEvent.Odds.Count == 0)
            {
                LogEventWithoutOddsSkipped(logger, originalEvent.Id);

                return null;
            }

            return upcomingEvent;
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            LogMalformedEventSkipped(logger, exception, originalEvent.Id);

            return null;
        }
    }

    private UpcomingEvent ToUpcomingEvent(Anonymous2 originalEvent)
    {
        return new UpcomingEvent
        {
            AwayTeam = originalEvent.Away_team ?? string.Empty,
            HomeTeam = originalEvent.Home_team ?? string.Empty,
            Id = originalEvent.Id ?? string.Empty,
            CommenceTime = originalEvent.Commence_time,
            Odds =
            [
                .. bookmakerConverter.ToOdds(originalEvent.Bookmakers,
                    homeTeam: originalEvent.Home_team,
                    awayTeam: originalEvent.Away_team)
            ]
        };
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Skipped event {Id}: already started")]
    private static partial void LogStartedEventSkipped(ILogger logger, string? id);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Skipped event {Id}: no usable odds")]
    private static partial void LogEventWithoutOddsSkipped(ILogger logger, string? id);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Skipped event {Id}")]
    private static partial void LogMalformedEventSkipped(ILogger logger, Exception exception, string? id);
}
