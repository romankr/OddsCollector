using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Infrastructure.Data;

internal static class EventPredictionBuilderExtensions
{
    public static EventPredictionBuilder SetSampleData(this EventPredictionBuilder builder)
    {
        return builder
            .SetId(SampleEvent.Id)
            .SetAwayTeam(SampleEvent.AwayTeam)
            .SetBookmaker(SampleEvent.Bookmaker1)
            .SetCommenceTime(SampleEvent.CommenceTime)
            .SetHomeTeam(SampleEvent.HomeTeam)
            .SetStrategy(SampleEvent.Strategy)
            .SetTraceId(SampleEvent.TraceId)
            .SetTimestamp(SampleEvent.Timestamp)
            .SetWinner(SampleEvent.Winner);
    }
}
