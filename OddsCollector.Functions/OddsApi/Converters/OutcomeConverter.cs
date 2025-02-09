using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class OutcomeConverter : IOutcomeConverter
{
    public Odd ToOdd(ICollection<Outcome>? outcomes, string? bookmaker, string awayTeam, string homeTeam)
    {
        ArgumentNullException.ThrowIfNull(outcomes);
        ArgumentException.ThrowIfNullOrEmpty(bookmaker);

        return new OddBuilder()
            .SetBookmaker(bookmaker)
            .SetHome(GetScore(outcomes, homeTeam))
            .SetAway(GetScore(outcomes, awayTeam))
            .SetDraw(GetScore(outcomes, OutcomeTypes.Draw)).Instance;
    }

    private static double? GetScore(IEnumerable<Outcome> outcomes, string oddType)
    {
        return outcomes.First(o => o.Name == oddType).Price;
    }
}
