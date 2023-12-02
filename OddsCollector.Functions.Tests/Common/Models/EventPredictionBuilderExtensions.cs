using OddsCollector.Functions.Models;
using OddsCollector.Functions.Strategies;

namespace OddsCollector.Functions.Tests.Common.Models;

internal static class EventPredictionBuilderExtensions
{
    public const string DefaultId = "4acd8f2675ca847ba33eea3664f6c0bb";
    public const string DefaultAwayTeam = "Liverpool";
    public const string DefaultBookmaker = "betclic";
    public const string DefaultHomeTeam = "Manchester City";
    public const string DefaultStrategy = nameof(AdjustedConsensusStrategy);
    public const string DefaultWinner = "Manchester City";
    public static readonly DateTime DefaultCommenceTime = new(2023, 11, 25, 12, 30, 0);
    public static readonly Guid DefaultTraceId = new("447b57dd-84bc-4e79-95d0-695f7493bf41");
    public static readonly DateTime DefaultTimestamp = new(2023, 11, 25, 15, 30, 0);

    public static EventPredictionBuilder SetDefaults(this EventPredictionBuilder builder)
    {
        return builder
            .SetId(DefaultId)
            .SetAwayTeam(DefaultAwayTeam)
            .SetBookmaker(DefaultBookmaker)
            .SetCommenceTime(DefaultCommenceTime)
            .SetHomeTeam(DefaultHomeTeam)
            .SetStrategy(DefaultStrategy)
            .SetTraceId(DefaultTraceId)
            .SetTimestamp(DefaultTimestamp)
            .SetWinner(DefaultWinner);
    }
}
