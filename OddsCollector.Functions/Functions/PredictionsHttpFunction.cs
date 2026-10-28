using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Functions;

internal sealed partial class PredictionsHttpFunction(ILogger<PredictionsHttpFunction> logger)
{
    private static readonly JsonSerializerOptions DocumentOptions = new() { PropertyNameCaseInsensitive = true };

    [Function(nameof(PredictionsHttpFunction))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get")]
        HttpRequestData request,
        [CosmosDBInput(
            "%CosmosDb:Database%",
            "%CosmosDb:EventPredictionsContainer%",
            Connection = "CosmosDb:Connection",
            SqlQuery = "SELECT TOP 100 * FROM p WHERE p.CommenceTime > GetCurrentDateTime() ORDER BY p.CommenceTime")]
        JsonElement[] documents,
        CancellationToken cancellationToken)
    {
        EventPrediction[] predictions = [.. ReadPredictions(documents)];

        var response = request.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(predictions, cancellationToken);

        return response;
    }

    private IEnumerable<EventPrediction> ReadPredictions(IEnumerable<JsonElement> documents)
    {
        foreach (var document in documents)
        {
            var prediction = TryReadPrediction(document);

            if (prediction is not null)
            {
                yield return prediction;
            }
        }
    }

    private EventPrediction? TryReadPrediction(JsonElement document)
    {
        try
        {
            return document.Deserialize<EventPrediction>(DocumentOptions)
                   ?? throw new JsonException("Document is null");
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException)
        {
            LogInvalidPredictionSkipped(logger, exception, GetId(document));

            return null;
        }
    }

    private static string? GetId(JsonElement document)
    {
        return document.ValueKind == JsonValueKind.Object
               && document.TryGetProperty("id", out var id)
               && id.ValueKind == JsonValueKind.String
            ? id.GetString()
            : null;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Skipped stored prediction {Id}: it cannot be read")]
    private static partial void LogInvalidPredictionSkipped(ILogger logger, Exception exception, string? id);
}
