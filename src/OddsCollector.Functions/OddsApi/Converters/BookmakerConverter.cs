using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal sealed partial class BookmakerConverter(ILogger<BookmakerConverter> logger, IMarketConverter converter)
    : IBookmakerConverter
{
    public IEnumerable<Odd> ToOdds(IEnumerable<Bookmakers> bookmakers, string homeTeam, string awayTeam)
    {
        ArgumentNullException.ThrowIfNull(bookmakers);
        ArgumentException.ThrowIfNullOrWhiteSpace(homeTeam);
        ArgumentException.ThrowIfNullOrWhiteSpace(awayTeam);

        return Iterate(bookmakers, homeTeam, awayTeam);
    }

    private IEnumerable<Odd> Iterate(IEnumerable<Bookmakers> bookmakers, string homeTeam, string awayTeam)
    {
        foreach (var bookmaker in bookmakers)
        {
            switch (converter.GetOdd(bookmaker.Markets, bookmaker.Key, homeTeam: homeTeam, awayTeam: awayTeam))
            {
                case Odd odd:
                    yield return odd;
                    break;
                case Rejection rejection:
                    LogBookmakerSkipped(logger, bookmaker.Key, homeTeam, awayTeam, rejection.Reason);
                    break;
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Skipped bookmaker {Bookmaker} for {HomeTeam} - {AwayTeam}: {Reason}")]
    private static partial void LogBookmakerSkipped(ILogger logger, string? bookmaker, string homeTeam,
        string awayTeam, string reason);
}
