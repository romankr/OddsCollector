using System.Text.Json.Serialization;

namespace OddsCollector.Functions.Models;

internal sealed class EventResult
{
    public DateTime CommenceTime { get; set; } = DateTime.MinValue;

    // cosmosdb requires lowercase
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    public string Winner { get; set; } = string.Empty;
}
