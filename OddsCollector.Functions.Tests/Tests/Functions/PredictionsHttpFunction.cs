using System.Net;
using System.Text.Json;
using Azure.Core.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
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
        var loggerStub = new FakeLogger<FunctionApp.PredictionsHttpFunction>();

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

        var function = new FunctionApp.PredictionsHttpFunction(loggerStub);

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

        loggerStub.Collector.Count.Should().Be(0);
    }

    [Test]
    public async Task Run_WithEmptyPredictions_ReturnsEmptyJsonArray()
    {
        var requestStub = HttpRequestDataFactory.Create();

        var function = new FunctionApp.PredictionsHttpFunction(new FakeLogger<FunctionApp.PredictionsHttpFunction>());

        var response = await function.Run(requestStub, []);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ReadBodyAsString().Should().Be("[]");
    }

    [Test]
    public async Task Run_WithSerializationException_ReturnsErrorHttpResponseAndLogsException()
    {
        // Arrange
        var loggerMock = new FakeLogger<FunctionApp.PredictionsHttpFunction>();

        const string expectedErrorMessage = "Failed to get predictions";
        const string expectedBody = """{"error":"Failed to get predictions"}""";

        var exception = new InvalidOperationException();

        var requestStub = HttpRequestDataFactory.Create(new PredictionsThrowingSerializer(exception));

        var function = new FunctionApp.PredictionsHttpFunction(loggerMock);

        // Act
        var response = await function.Run(requestStub, [new EventPrediction()]);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        response.Headers.GetValues("Content-Type").Should().ContainSingle()
            .Which.Should().StartWith("application/json");

        response.ReadBodyAsString().Should().Be(expectedBody);

        loggerMock.Collector.Count.Should().Be(1);
        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Error);
        loggerMock.LatestRecord.Message.Should().Be(expectedErrorMessage);
        loggerMock.LatestRecord.Exception.Should().Be(exception);
    }

    /// <summary>
    /// Fails for the predictions payload only, so the error body can still be written.
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
