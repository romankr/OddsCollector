using System.ComponentModel.DataAnnotations;

namespace OddsCollector.Common.KeyVault.Configuration;

public class KeyVaultOptions
{
    [Required(AllowEmptyStrings = false)] public string Name { get; set; } = string.Empty;
}
