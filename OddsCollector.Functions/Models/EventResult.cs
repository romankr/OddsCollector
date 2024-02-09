using System.Text.Json.Serialization;

namespace OddsCollector.Functions.Models;

internal class EventResult
{
    public DateTime CommenceTime { get; set; } = DateTime.MinValue;

    // fixed id for cosmosdb
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.MinValue;
    public Guid TraceId { get; set; } = Guid.Empty;
    public string Winner { get; set; } = string.Empty;
}
