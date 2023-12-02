using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Common.Models;

internal static class UpcomingEventBuilderExtensions
{
    public const string DefaultAwayTeam = "Liverpool";
    public const string DefaultHomeTeam = "Manchester City";
    public const string DefaultId = "4acd8f2675ca847ba33eea3664f6c0bb";
    public static readonly DateTime DefaultCommenceTime = new(2023, 11, 25, 12, 30, 0);
    public static readonly DateTime DefaultTimestamp = new(2023, 11, 25, 15, 30, 0);
    public static readonly Guid DefaultTraceId = new("447b57dd-84bc-4e79-95d0-695f7493bf41");

    public static readonly IEnumerable<Odd> DefaultOdds = new List<Odd>
    {
        new OddBuilder().SetDefaults1().Instance,
        new OddBuilder().SetDefaults2().Instance,
        new OddBuilder().SetDefaults3().Instance
    };

    public static UpcomingEventBuilder SetDefaults(this UpcomingEventBuilder builder)
    {
        return builder
            .SetAwayTeam(DefaultAwayTeam)
            .SetCommenceTime(DefaultCommenceTime)
            .SetHomeTeam(DefaultHomeTeam)
            .SetId(DefaultId)
            .SetTimestamp(DefaultTimestamp)
            .SetTraceId(DefaultTraceId)
            .SetOdds(DefaultOdds);
    }
}
