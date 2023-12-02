using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Common.Models;

internal static class OddsBuilderExtensions
{
    public const double DefaultAway1 = 4.08;
    public const string DefaultBookmaker1 = "betclic";
    public const double DefaultDraw1 = 3.82;
    public const double DefaultHome1 = 1.8;

    public const double DefaultAway2 = 4.33;
    public const string DefaultBookmaker2 = "sport888";
    public const double DefaultDraw2 = 4.33;
    public const double DefaultHome2 = 1.7;

    public const double DefaultAway3 = 4.5;
    public const string DefaultBookmaker3 = "mybookieag";
    public const double DefaultDraw3 = 4.5;
    public const double DefaultHome3 = 1.67;

    public static OddBuilder SetDefaults1(this OddBuilder builder)
    {
        return builder
            .SetAway(DefaultAway1)
            .SetBookmaker(DefaultBookmaker1)
            .SetHome(DefaultHome1)
            .SetDraw(DefaultDraw1);
    }

    public static OddBuilder SetDefaults2(this OddBuilder builder)
    {
        return builder
            .SetAway(DefaultAway2)
            .SetBookmaker(DefaultBookmaker2)
            .SetHome(DefaultHome2)
            .SetDraw(DefaultDraw2);
    }

    public static OddBuilder SetDefaults3(this OddBuilder builder)
    {
        return builder
            .SetAway(DefaultAway3)
            .SetBookmaker(DefaultBookmaker3)
            .SetHome(DefaultHome3)
            .SetDraw(DefaultDraw3);
    }
}
