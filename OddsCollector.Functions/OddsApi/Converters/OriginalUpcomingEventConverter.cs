using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class OriginalUpcomingEventConverter(IBookmakerConverter bookmakerConverter)
    : IOriginalUpcomingEventConverter
{
    public IEnumerable<UpcomingEvent> ToUpcomingEvents(ICollection<Anonymous2>? events)
    {
        CheckParameters(events);

        foreach (var originalEvent in events!)
        {
            yield return ToUpcomingEvent(originalEvent);
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

    private static void CheckParameters(ICollection<Anonymous2>? events)
    {
        ArgumentNullException.ThrowIfNull(events);
    }
}
