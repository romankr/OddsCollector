using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class OddConverter : IOddConverter
{
    public Odd ToOdd(ICollection<Outcome>? outcomes, string? bookmaker, string awayTeam, string homeTeam)
    {
        ArgumentNullException.ThrowIfNull(outcomes);
        ArgumentException.ThrowIfNullOrWhiteSpace(bookmaker);

        return new Odd
        {
            Bookmaker = bookmaker,
            Home = GetPrice(outcomes, homeTeam),
            Away = GetPrice(outcomes, awayTeam),
            Draw = GetPrice(outcomes, OutcomeTypes.Draw)
        };
    }

    private static double GetPrice(IEnumerable<Outcome> outcomes, string name)
    {
        return outcomes.First(o => o.Name == name).Price
               ?? throw new ArgumentException($"Outcome {name} has no price", nameof(outcomes));
    }
}
