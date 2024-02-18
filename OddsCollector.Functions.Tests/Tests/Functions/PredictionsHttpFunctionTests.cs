using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Functions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Tests.Data;

namespace OddsCollector.Functions.Tests.Tests.Functions;

internal class PredictionsHttpFunctionTests
{
    [Test]
    public void Run_WithValidArguments_ReturnsValidResponse()
    {
        // Arrange
        var loggerStub = Substitute.For<ILogger<PredictionsHttpFunction>>();

        var function = new PredictionsHttpFunction(loggerStub);

        var contextStub = Substitute.For<FunctionContext>();

        var headersMock = Substitute.For<HttpHeadersCollection>();

        var responseStream = new MemoryStream();

        var responseMock = Substitute.For<HttpResponseData>(contextStub);
        responseMock.Headers.Returns(headersMock);
        responseMock.Body.Returns(responseStream);

        var requestStub = Substitute.For<HttpRequestData>(contextStub);

        requestStub.CreateResponse().Returns(responseMock);

        var timestamp = DateTime.UtcNow;

        var predictions = new[]
        {
            new EventPredictionBuilder().SetSampleData().SetTimestamp(timestamp).Instance,
        };

        // Act
        var response = function.Run(requestStub, predictions);

        // Assert
        response.Should().NotBeNull();
        responseMock.StatusCode.Should().Be(HttpStatusCode.OK);

        using var reader = new StreamReader(responseMock.Body);
        responseMock.Body.Seek(0, SeekOrigin.Begin);
        var text = reader.ReadToEnd();

        var deserialized = JsonSerializer.Deserialize<EventPrediction[]>(text);
        deserialized.Should().NotBeNull().And.HaveCount(1);
        deserialized![0].Timestamp.Should().Be(timestamp);
    }
}
