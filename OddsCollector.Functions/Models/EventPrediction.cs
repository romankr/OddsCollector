using System.Text.Json.Serialization;

namespace OddsCollector.Functions.Models;

internal sealed record EventPrediction
{
    public required string AwayTeam { get; init => field = Guard.NotBlank(value, nameof(AwayTeam)); }

    public required DateTime CommenceTime { get; init => field = UtcDateTime.Require(value, nameof(CommenceTime)); }

    public required string HomeTeam { get; init => field = Guard.NotBlank(value, nameof(HomeTeam)); }

    // Cosmos DB requires the id in lowercase.
    [JsonPropertyName("id")]
    public required string Id { get; init => field = Guard.NotBlank(value, nameof(Id)); }

    public required string Outcome { get; init => field = Guard.NotBlank(value, nameof(Outcome)); }
}
