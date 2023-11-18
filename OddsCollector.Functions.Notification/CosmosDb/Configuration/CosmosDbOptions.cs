using System.ComponentModel.DataAnnotations;

namespace OddsCollector.Functions.Notification.CosmosDb.Configuration;

public class CosmosDbOptions
{
    [Required(AllowEmptyStrings = false)] public string Connection { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)] public string Container { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)] public string Database { get; set; } = string.Empty;
}
