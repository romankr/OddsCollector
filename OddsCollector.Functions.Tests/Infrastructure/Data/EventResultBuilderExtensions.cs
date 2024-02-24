using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Infrastructure.Data;

internal static class EventResultBuilderExtensions
{
    public static EventResultBuilder SetSampleData(this EventResultBuilder builder)
    {
        return builder
            .SetId(SampleEvent.Id)
            .SetWinner(SampleEvent.Winner)
            .SetCommenceTime(SampleEvent.CommenceTime)
            .SetTimestamp(SampleEvent.Timestamp)
            .SetTraceId(SampleEvent.TraceId);
    }
}
