using System.Diagnostics.CodeAnalysis;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.OddsApi.Converters;

internal interface IMarketConverter
{
    bool TryGetOdd(ICollection<Markets2>? markets, string? bookmaker, string homeTeam, string awayTeam,
        [NotNullWhen(true)] out Odd? odd);
}
