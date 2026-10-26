using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class BookmakerConverter(ILogger<BookmakerConverter> logger, IMarketConverter converter)
    : IBookmakerConverter
{
    public IEnumerable<Odd> ToOdds(ICollection<Bookmakers>? bookmakers, string? homeTeam, string? awayTeam)
    {
        ArgumentNullException.ThrowIfNull(bookmakers);
        ArgumentException.ThrowIfNullOrWhiteSpace(homeTeam);
        ArgumentException.ThrowIfNullOrWhiteSpace(awayTeam);

        return Iterate(bookmakers, homeTeam, awayTeam);
    }

    private IEnumerable<Odd> Iterate(ICollection<Bookmakers> bookmakers, string homeTeam, string awayTeam)
    {
        foreach (var bookmaker in bookmakers)
        {
            var odd = TryToOdd(bookmaker, homeTeam, awayTeam);

            if (odd is not null)
            {
                yield return odd;
            }
        }
    }

    private Odd? TryToOdd(Bookmakers bookmaker, string homeTeam, string awayTeam)
    {
        try
        {
            return converter.ToOdd(bookmaker.Markets, bookmaker.Key, homeTeam: homeTeam, awayTeam: awayTeam);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            logger.LogWarning(exception, "Skipped bookmaker {Bookmaker} for {HomeTeam} - {AwayTeam}",
                bookmaker.Key, homeTeam, awayTeam);

            return null;
        }
    }
}
