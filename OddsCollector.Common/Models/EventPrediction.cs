using System.Text.Json.Serialization;

namespace OddsCollector.Common.Models;

public class EventPrediction
{
    // duplicating information to avoid complex queries to cosmosdb
    public string AwayTeam { get; set; } = string.Empty;

    public string Bookmaker { get; set; } = string.Empty;

    // duplicating information to avoid complex queries to cosmosdb
    public DateTime CommenceTime { get; set; } = DateTime.MinValue;

    // duplicating information to avoid complex queries to cosmosdb
    public string HomeTeam { get; set; } = string.Empty;

    // fix for cosmosdb
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    public string Strategy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.MinValue;
    public Guid TraceId { get; set; } = Guid.Empty;
    public string Winner { get; set; } = string.Empty;
}
