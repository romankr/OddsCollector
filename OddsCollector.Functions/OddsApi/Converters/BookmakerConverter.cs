using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed partial class BookmakerConverter(ILogger<BookmakerConverter> logger, IMarketConverter converter)
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
            if (converter.TryGetOdd(bookmaker.Markets, bookmaker.Key, homeTeam: homeTeam, awayTeam: awayTeam,
                    out var odd))
            {
                return odd;
            }

            LogBookmakerWithoutHeadToHeadSkipped(logger, bookmaker.Key, homeTeam, awayTeam);

            return null;
        }
        catch (ArgumentException exception)
        {
            LogBookmakerSkipped(logger, exception, bookmaker.Key, homeTeam, awayTeam);

            return null;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Skipped bookmaker {Bookmaker} for {HomeTeam} - {AwayTeam}: no head-to-head market")]
    private static partial void LogBookmakerWithoutHeadToHeadSkipped(ILogger logger, string? bookmaker,
        string homeTeam, string awayTeam);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Skipped bookmaker {Bookmaker} for {HomeTeam} - {AwayTeam}")]
    private static partial void LogBookmakerSkipped(ILogger logger, Exception exception, string? bookmaker,
        string homeTeam, string awayTeam);
}
