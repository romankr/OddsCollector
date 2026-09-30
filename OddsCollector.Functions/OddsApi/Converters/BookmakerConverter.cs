using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed class BookmakerConverter(ILogger<BookmakerConverter> logger, IMarketConverter converter)
    : IBookmakerConverter
{
    public IEnumerable<Odd> ToOdds(ICollection<Bookmakers>? bookmakers, string? awayTeam, string? homeTeam)
    {
        ArgumentNullException.ThrowIfNull(bookmakers);
        ArgumentException.ThrowIfNullOrWhiteSpace(awayTeam);
        ArgumentException.ThrowIfNullOrWhiteSpace(homeTeam);

        return Iterate(bookmakers, awayTeam, homeTeam);
    }

    private IEnumerable<Odd> Iterate(ICollection<Bookmakers> bookmakers, string awayTeam, string homeTeam)
    {
        foreach (var bookmaker in bookmakers)
        {
            var odd = TryToOdd(bookmaker, awayTeam, homeTeam);

            if (odd is not null)
            {
                yield return odd;
            }
        }
    }

    private Odd? TryToOdd(Bookmakers bookmaker, string awayTeam, string homeTeam)
    {
        try
        {
            return converter.ToOdd(bookmaker.Markets, bookmaker.Key, awayTeam, homeTeam);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            logger.LogWarning(exception, "Skipped bookmaker {Bookmaker} for {HomeTeam} - {AwayTeam}",
                bookmaker.Key, homeTeam, awayTeam);

            return null;
        }
    }
}
