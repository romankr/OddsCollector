using System.Globalization;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class OddConverter : IOddConverter
{
    private const string DrawOutcomeName = "Draw";

    private const double BreakEvenDecimalOdds = 1;

    public Conversion<Odd> ToOdd(ICollection<Outcome>? outcomes, string? bookmaker, string homeTeam,
        string awayTeam)
    {
        if (string.IsNullOrWhiteSpace(bookmaker))
        {
            return new Rejection("no bookmaker key");
        }

        if (outcomes is null)
        {
            return new Rejection("no outcomes");
        }

        var homePrice = GetPrice(outcomes, homeTeam);
        var drawPrice = GetPrice(outcomes, DrawOutcomeName);
        var awayPrice = GetPrice(outcomes, awayTeam);

        return (homePrice, drawPrice, awayPrice) switch
        {
            (Rejection rejection, _, _) => rejection,
            (_, Rejection rejection, _) => rejection,
            (_, _, Rejection rejection) => rejection,
            (double home, double draw, double away) =>
                new Odd { Bookmaker = bookmaker, Home = home, Draw = draw, Away = away }
        };
    }

    private static Conversion<double> GetPrice(IEnumerable<Outcome> outcomes, string name)
    {
        var outcome = outcomes.FirstOrDefault(o => o.Name == name);

        if (outcome is null)
        {
            return new Rejection($"no {name} outcome");
        }

        if (outcome.Price is not { } price)
        {
            return new Rejection($"no price for {name}");
        }

        if (!double.IsFinite(price) || price <= BreakEvenDecimalOdds)
        {
            return new Rejection($"unusable price for {name}: {price.ToString(CultureInfo.InvariantCulture)}");
        }

        return price;
    }
}
