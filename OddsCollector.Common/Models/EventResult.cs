using System.Text.Json.Serialization;

namespace OddsCollector.Common.Models;

public class EventResult
{
    public DateTime? CommenceTime { get; init; }

    // fix for cosmosdb
    [JsonPropertyName("id")] public string? Id { get; init; }

    public DateTime? Timestamp { get; init; }
    public Guid? TraceId { get; init; }
    public string? Winner { get; init; }
}
