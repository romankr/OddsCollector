using System.Text.Json;
using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Processors;

internal class PredictionHttpRequestProcessor : IPredictionHttpRequestProcessor
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public string Serialize(EventPrediction[] predictions)
    {
        return JsonSerializer.Serialize(predictions, SerializerOptions);
    }
}
