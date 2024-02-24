using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Infrastructure.Data;

internal static class OddsBuilderExtensions
{
    public static OddBuilder SetSampleData1(this OddBuilder builder)
    {
        return builder
            .SetAway(SampleEvent.AwayOdd1)
            .SetBookmaker(SampleEvent.Bookmaker1)
            .SetHome(SampleEvent.HomeOdd1)
            .SetDraw(SampleEvent.DrawOdd1);
    }

    public static OddBuilder SetSampleData2(this OddBuilder builder)
    {
        return builder
            .SetAway(SampleEvent.AwayOdd2)
            .SetBookmaker(SampleEvent.Bookmaker2)
            .SetHome(SampleEvent.HomeOdd2)
            .SetDraw(SampleEvent.DrawOdd2);
    }

    public static OddBuilder SetSampleData3(this OddBuilder builder)
    {
        return builder
            .SetAway(SampleEvent.AwayOdd3)
            .SetBookmaker(SampleEvent.Bookmaker3)
            .SetHome(SampleEvent.HomeOdd3)
            .SetDraw(SampleEvent.DrawOdd3);
    }
}
