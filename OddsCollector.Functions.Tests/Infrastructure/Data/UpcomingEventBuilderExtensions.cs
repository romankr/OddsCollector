using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Infrastructure.Data;

internal static class UpcomingEventBuilderExtensions
{
    public static UpcomingEventBuilder SetSampleData(this UpcomingEventBuilder builder)
    {
        return builder
            .SetAwayTeam(SampleEvent.AwayTeam)
            .SetCommenceTime(SampleEvent.CommenceTime)
            .SetHomeTeam(SampleEvent.HomeTeam)
            .SetId(SampleEvent.Id)
            .SetOdds(SampleEvent.Odds);
    }
}
