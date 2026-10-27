using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core.Serialization;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using OddsCollector.Functions.Functions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.UnitTests.Infrastructure.Http;
using OddsCollector.Functions.UnitTests.Infrastructure.Models;

namespace OddsCollector.Functions.UnitTests.Tests.Functions;

internal sealed class PredictionsHttpFunctionTests
{
    private const string BrokenId = "broken";

    [Test]
    public async Task Run_WithPredictions_ReturnsThemAsJson()
    {
        // Arrange
        EventPrediction[] predictions =
        [
            new()
            {
                Id = "id",
                AwayTeam = "awayTeam",
                HomeTeam = "homeTeam",
                Outcome = OutcomeType.HomeTeam,
                CommenceTime = new DateTime(2026, 9, 22, 18, 0, 0, DateTimeKind.Utc)
            }
        ];

        var requestStub = HttpRequestDataFactory.Create();

        var function = CreateFunction();

        // Act
        var response = await function.Run(requestStub, ToDocuments(predictions), CancellationToken.None);

        // Assert
        using var scope = new AssertionScope();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues("Content-Type").Should().ContainSingle()
            .Which.Should().StartWith("application/json");
        response.ReadBodyAsString().Should().Be(JsonSerializer.Serialize(predictions));
    }

    [Test]
    public async Task Run_WithNoPredictions_ReturnsEmptyJsonArray()
    {
        // Arrange
        var requestStub = HttpRequestDataFactory.Create();

        var function = CreateFunction();

        // Act
        var response = await function.Run(requestStub, [], CancellationToken.None);

        // Assert
        using var scope = new AssertionScope();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ReadBodyAsString().Should().Be("[]");
    }

    [Test]
    public async Task Run_WithSerializationException_LetsItReachTheHost()
    {
        // Arrange
        var expectedException = new InvalidOperationException();

        var requestStub = HttpRequestDataFactory.Create(new PredictionsThrowingSerializer(expectedException));

        var function = CreateFunction();

        var documents = ToDocuments(ValidModels.CreateEventPrediction());

        // Act
        var action = () => function.Run(requestStub, documents, CancellationToken.None);

        // Assert
        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(expectedException);
    }

    [TestCase("Outcome", "\"Unknown\"", TestName = "Run_WithDocumentWithUnknownOutcome_SkipsItAndReturnsTheRest")]
    [TestCase("HomeTeam", "\" \"", TestName = "Run_WithDocumentWithBlankTeam_SkipsItAndReturnsTheRest")]
    [TestCase("CommenceTime", null, TestName = "Run_WithDocumentWithoutCommenceTime_SkipsItAndReturnsTheRest")]
    public async Task Run_WithUnreadableDocument_SkipsItAndReturnsTheRest(string property, string? jsonValue)
    {
        // Arrange
        var prediction = ValidModels.CreateEventPrediction();
        var brokenDocument = CreateDocumentWith(BrokenId, property, jsonValue);

        var requestStub = HttpRequestDataFactory.Create();

        var loggerMock = new FakeLogger<PredictionsHttpFunction>();

        var function = CreateFunction(loggerMock);

        JsonElement[] documents = [brokenDocument, ToDocuments(prediction)[0]];

        // Act
        var response = await function.Run(requestStub, documents, CancellationToken.None);

        // Assert
        using var scope = new AssertionScope();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ReadBodyAsString().Should().Be(JsonSerializer.Serialize(new[] { prediction }));

        var record = loggerMock.Collector.GetSnapshot().Should().ContainSingle().Subject;
        record.Level.Should().Be(LogLevel.Warning);
        record.Message.Should().Be($"Skipped stored prediction {BrokenId}: it cannot be read");
    }

    [Test]
    public async Task Run_WithDocumentWithoutId_SkipsItWithoutId()
    {
        // Arrange
        var requestStub = HttpRequestDataFactory.Create();

        var loggerMock = new FakeLogger<PredictionsHttpFunction>();

        var function = CreateFunction(loggerMock);

        JsonElement[] documents = [JsonSerializer.Deserialize<JsonElement>("[]")];

        // Act
        var response = await function.Run(requestStub, documents, CancellationToken.None);

        // Assert
        using var scope = new AssertionScope();

        response.ReadBodyAsString().Should().Be("[]");
        loggerMock.Collector.GetSnapshot().Should().ContainSingle()
            .Which.Message.Should().Be("Skipped stored prediction (null): it cannot be read");
    }

    private static PredictionsHttpFunction CreateFunction(ILogger<PredictionsHttpFunction>? logger = null)
    {
        return new PredictionsHttpFunction(logger ?? NullLogger<PredictionsHttpFunction>.Instance);
    }

    private static JsonElement CreateDocumentWith(string id, string property, string? jsonValue)
    {
        var document = JsonSerializer.SerializeToNode(ValidModels.CreateEventPrediction() with { Id = id })!
            .AsObject();

        // A null value removes the property, so the document misses it entirely.
        document.Remove(property);

        if (jsonValue is not null)
        {
            document[property] = JsonNode.Parse(jsonValue);
        }

        return JsonSerializer.SerializeToElement(document);
    }

    private static JsonElement[] ToDocuments(params EventPrediction[] predictions)
    {
        return [.. predictions.Select(p => JsonSerializer.SerializeToElement(p))];
    }

    private sealed class PredictionsThrowingSerializer(Exception exception) : ObjectSerializer
    {
        private readonly JsonObjectSerializer _inner = new();

        public override void Serialize(Stream stream, object? value, Type inputType,
            CancellationToken cancellationToken)
        {
            ThrowIfPredictions(value);
            _inner.Serialize(stream, value, inputType, cancellationToken);
        }

        public override ValueTask SerializeAsync(Stream stream, object? value, Type inputType,
            CancellationToken cancellationToken)
        {
            ThrowIfPredictions(value);
            return _inner.SerializeAsync(stream, value, inputType, cancellationToken);
        }

        public override object? Deserialize(Stream stream, Type returnType, CancellationToken cancellationToken)
        {
            return _inner.Deserialize(stream, returnType, cancellationToken);
        }

        public override ValueTask<object?> DeserializeAsync(Stream stream, Type returnType,
            CancellationToken cancellationToken)
        {
            return _inner.DeserializeAsync(stream, returnType, cancellationToken);
        }

        private void ThrowIfPredictions(object? value)
        {
            if (value is EventPrediction[])
            {
                throw exception;
            }
        }
    }
}
