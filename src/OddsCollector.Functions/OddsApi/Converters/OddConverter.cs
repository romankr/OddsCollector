using System.Globalization;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class OddConverter : IOddConverter
{
    private const string DrawOutcomeName = "Draw";

    // Decimal odds of 1 or less pay back nothing over the stake, so they are not real quotes.
    private const double BreakEvenDecimalOdds = 1;

    public Odd ToOdd(ICollection<Outcome>? outcomes, string? bookmaker, string homeTeam, string awayTeam)
    {
        ArgumentNullException.ThrowIfNull(outcomes);
        ArgumentException.ThrowIfNullOrWhiteSpace(bookmaker);

        return new Odd
        {
            Bookmaker = bookmaker,
            Home = GetPrice(outcomes, homeTeam),
            Away = GetPrice(outcomes, awayTeam),
            Draw = GetPrice(outcomes, DrawOutcomeName)
        };
    }

    private static double GetPrice(IEnumerable<Outcome> outcomes, string name)
    {
        var outcome = outcomes.FirstOrDefault(o => o.Name == name)
                      ?? throw new ArgumentException($"Outcome {name} is missing", nameof(outcomes));

        var price = outcome.Price ?? throw new ArgumentException($"Outcome {name} has no price", nameof(outcomes));

        if (!double.IsFinite(price) || price <= BreakEvenDecimalOdds)
        {
            throw new ArgumentException(
                $"Outcome {name} has an unusable price: {price.ToString(CultureInfo.InvariantCulture)}",
                nameof(outcomes));
        }

        return price;
    }
}
