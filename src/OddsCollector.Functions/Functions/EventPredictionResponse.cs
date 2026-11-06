using System.Text.Json.Serialization;
using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Functions;

// A prediction as PredictionsHttpFunction returns it. The kick-off time is an ISO 8601 UTC string
// (2026-09-22T18:00:00Z): the Unix seconds of the stored EventPrediction are a storage format for Cosmos DB queries,
// not part of the API.
internal sealed record EventPredictionResponse
{
    public required string AwayTeam { get; init; }

    public required DateTime CommenceTime { get; init; }

    public required string HomeTeam { get; init; }

    // The same name as in the stored document, so that the response keeps its shape.
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    public required OutcomeType Outcome { get; init; }

    public static EventPredictionResponse FromPrediction(EventPrediction prediction)
    {
        ArgumentNullException.ThrowIfNull(prediction);

        return new EventPredictionResponse
        {
            AwayTeam = prediction.AwayTeam,
            CommenceTime = prediction.CommenceTime,
            HomeTeam = prediction.HomeTeam,
            Id = prediction.Id,
            Outcome = prediction.Outcome
        };
    }
}
