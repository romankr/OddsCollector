using System.Text.Json.Serialization;

namespace OddsCollector.Common.Models;

public class EventPrediction
{
    public string? AwayTeam { get; init; }
    public string? Bookmaker { get; init; }
    public DateTime? CommenceTime { get; init; }

    public string? HomeTeam { get; init; }

    // fix for cosmosdb
    [JsonPropertyName("id")] public string? Id { get; init; }

    public string? Strategy { get; init; }
    public DateTime? Timestamp { get; init; }
    public Guid? TraceId { get; init; }
    public string? Winner { get; init; }
}
