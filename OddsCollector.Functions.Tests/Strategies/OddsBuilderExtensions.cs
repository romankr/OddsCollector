using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Strategies;

internal static class OddsBuilderExtensions
{
    public const double DefaultAway = 4.08;
    public const string DefaultBookmaker = "betclic";
    public const double DefaultDraw = 3.82;
    public const double DefaultHome = 1.8;

    public static OddBuilder SetDefaults(this OddBuilder builder)
    {
        return builder
            .SetAway(DefaultAway)
            .SetBookmaker(DefaultBookmaker)
            .SetHome(DefaultHome)
            .SetDraw(DefaultDraw);
    }
}
