using System.Net;
using System.Text.Json;
using Azure.Core.Serialization;
using FluentAssertions.Execution;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Tests.Infrastructure.Http;
using OddsCollector.Functions.Tests.Infrastructure.Models;
using FunctionApp = OddsCollector.Functions.Functions;

namespace OddsCollector.Functions.Tests.Tests.Functions;

internal sealed class PredictionsHttpFunction
{
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
                Outcome = OutcomeTypes.HomeTeam,
                CommenceTime = new DateTime(2026, 9, 22, 18, 0, 0, DateTimeKind.Utc)
            }
        ];

        var requestStub = HttpRequestDataFactory.Create();

        var function = new FunctionApp.PredictionsHttpFunction();

        // Act
        var response = await function.Run(requestStub, predictions);

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

        var function = new FunctionApp.PredictionsHttpFunction();

        // Act
        var response = await function.Run(requestStub, []);

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

        var function = new FunctionApp.PredictionsHttpFunction();

        // Act
        var action = () => function.Run(requestStub, [ValidModels.CreateEventPrediction()]);

        // Assert: the failure reaches the host, which logs it and answers with a 500.
        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(expectedException);
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
