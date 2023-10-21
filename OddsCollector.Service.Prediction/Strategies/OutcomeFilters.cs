using OddsCollector.Common.ServiceBus.Models;

namespace OddsCollector.Service.Prediction.Strategies;

internal static class OutcomeFilters
{
    public static double? Draw(Odd? odd)
    {
        if (odd is null)
        {
            throw new ArgumentNullException(nameof(odd));
        }

        return odd.Draw;
    }

    public static double? AwayTeamWins(Odd? odd)
    {
        if (odd is null)
        {
            throw new ArgumentNullException(nameof(odd));
        }

        return odd.Away;
    }

    public static double? HomeTeamWins(Odd? odd)
    {
        if (odd is null)
        {
            throw new ArgumentNullException(nameof(odd));
        }

        return odd.Home;
    }
}
