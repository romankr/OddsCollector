using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace OddsCollector.Common.OddsApi.Configuration;

public class OddsApiClientOptions
{
    [Required]
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
    public HashSet<string> Leagues { get; set; } = new();

    public void SetLeagues(string? leagues)
    {
        if (string.IsNullOrEmpty(leagues))
        {
            throw new ArgumentException($"{nameof(leagues)} cannot be null or empty", nameof(leagues));
        }

        Leagues = leagues.Split(";").ToHashSet();
    }
}
