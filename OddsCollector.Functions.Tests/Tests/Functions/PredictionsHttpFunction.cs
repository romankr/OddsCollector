using System.Net;
using System.Text.Json;
using Azure.Core.Serialization;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Tests.Infrastructure.Http;
using FunctionApp = OddsCollector.Functions.Functions;

namespace OddsCollector.Functions.Tests.Tests.Functions;

internal sealed class PredictionsHttpFunction
{
    [Test]
    public async Task Run_WithPredictions_ReturnsSuccessfulHttpResponse()
    {
        // Arrange
        EventPrediction[] predictions =
        [
            new()
            {
                Id = "1",
                AwayTeam = "Away",
                HomeTeam = "Home",
                Winner = "Home",
                CommenceTime = new DateTime(2026, 9, 22, 18, 0, 0, DateTimeKind.Utc)
            }
        ];

        var requestStub = HttpRequestDataFactory.Create();

        var function = new FunctionApp.PredictionsHttpFunction();

        // Act
        var response = await function.Run(requestStub, predictions);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        response.Headers.GetValues("Content-Type").Should().ContainSingle()
            .Which.Should().StartWith("application/json");

        var body = response.ReadBodyAsString();

        body.Should().Be(JsonSerializer.Serialize(predictions));
        JsonSerializer.Deserialize<EventPrediction[]>(body).Should().BeEquivalentTo(predictions);
    }

    [Test]
    public async Task Run_WithEmptyPredictions_ReturnsEmptyJsonArray()
    {
        var requestStub = HttpRequestDataFactory.Create();

        var function = new FunctionApp.PredictionsHttpFunction();

        var response = await function.Run(requestStub, []);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ReadBodyAsString().Should().Be("[]");
    }

    [Test]
    public async Task Run_WithSerializationException_Throws()
    {
        // Arrange
        var exception = new InvalidOperationException();

        var requestStub = HttpRequestDataFactory.Create(new PredictionsThrowingSerializer(exception));

        var function = new FunctionApp.PredictionsHttpFunction();

        // Act
        var action = () => function.Run(requestStub, [new EventPrediction()]);

        // Assert: the failure reaches the host, which logs it and answers with a 500.
        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Should().Be(exception);
    }

    /// <summary>
    /// Fails for the predictions payload only.
    /// </summary>
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
