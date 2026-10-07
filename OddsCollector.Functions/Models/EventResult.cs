using System.Text.Json.Serialization;

namespace OddsCollector.Functions.Models;

internal sealed record EventResult
{
    public required DateTime CommenceTime { get; init => field = UtcDateTime.Require(value, nameof(CommenceTime)); }

    // Cosmos DB requires the id in lowercase.
    [JsonPropertyName("id")]
    public required string Id { get; init => field = Guard.NotBlank(value, nameof(Id)); }

    public required string Outcome { get; init => field = Guard.NotBlank(value, nameof(Outcome)); }
}
