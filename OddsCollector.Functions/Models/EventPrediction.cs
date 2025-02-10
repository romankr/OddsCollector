using System.Text.Json.Serialization;

namespace OddsCollector.Functions.Models;

internal sealed class EventPrediction
{
    public string AwayTeam { get; set; } = string.Empty;

    public DateTime CommenceTime { get; set; } = DateTime.MinValue;

    public string HomeTeam { get; set; } = string.Empty;

    // cosmosdb requires lowercase
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    public string Winner { get; set; } = string.Empty;
}
