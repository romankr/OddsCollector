using System.Text.Json;
using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Tests.Processors;

internal sealed class PredictionHttpRequestProcessor
{
    [Test]
    public void Serialize_WithPredictions_ReturnsSerializedAsArray()
    {
        var processor = new OddsCollector.Functions.Processors.PredictionHttpRequestProcessor();
        var prediction = new EventPrediction();
        var serialized = processor.Serialize([prediction]);

        var deserialized = JsonSerializer.Deserialize<EventPrediction[]>(serialized);

        deserialized.Should().NotBeNullOrEmpty().And.HaveCount(1);
        deserialized![0].Should().NotBeNull().And.BeEquivalentTo(prediction);
    }
}
