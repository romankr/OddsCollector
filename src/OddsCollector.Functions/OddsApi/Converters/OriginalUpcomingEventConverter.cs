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

            switch (ToUpcomingEvent(originalEvent))
            {
                case UpcomingEvent upcomingEvent:
                    yield return upcomingEvent;
                    break;
                case Rejection rejection:
                    LogEventSkipped(logger, originalEvent.Id, rejection.Reason);
                    break;
            }
        }
    }

    private Conversion<UpcomingEvent> ToUpcomingEvent(Anonymous2 originalEvent)
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

        if (originalEvent.Bookmakers is null)
        {
            return new Rejection("no bookmakers");
        }

        IReadOnlyList<Odd> odds =
        [
            .. bookmakerConverter.ToOdds(originalEvent.Bookmakers,
                homeTeam: originalEvent.Home_team,
                awayTeam: originalEvent.Away_team)
        ];

        if (odds.Count == 0)
        {
            return new Rejection("no usable odds");
        }

        return new UpcomingEvent
        {
            AwayTeam = originalEvent.Away_team,
            HomeTeam = originalEvent.Home_team,
            Id = originalEvent.Id,
            CommenceTime = originalEvent.Commence_time,
            Odds = odds
        };
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Skipped event {Id}: already started")]
    private static partial void LogStartedEventSkipped(ILogger logger, string? id);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Skipped event {Id}: {Reason}")]
    private static partial void LogEventSkipped(ILogger logger, string? id, string reason);
}
