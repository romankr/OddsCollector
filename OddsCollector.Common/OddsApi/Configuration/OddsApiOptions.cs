using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace OddsCollector.Common.OddsApi.Configuration;

public class OddsApiOptions
{
    [Required]
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
    public HashSet<string> Leagues { get; set; } = new();
}
