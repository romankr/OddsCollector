using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Functions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Tests.Common.Models;

namespace OddsCollector.Functions.Tests.Tests.Functions;

[Parallelizable(ParallelScope.All)]
internal class PredictionsHttpFunctionTests
{
    [Test]
    public void Constructor_WithValidDependencies_ReturnsNewInstance()
    {
        var loggerStub = Substitute.For<ILogger<PredictionsHttpFunction>>();

        var function = new PredictionsHttpFunction(loggerStub);

        function.Should().NotBeNull();
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsException()
    {
        var action = () =>
        {
            _ = new PredictionsHttpFunction(null);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void Run_WithValidArguments_ReturnsValidResponse()
    {
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
            new EventPredictionBuilder().SetDefaults().SetCommenceTime(timestamp.AddDays(-2)).Instance,
            new EventPredictionBuilder().SetDefaults()
                .SetCommenceTime(timestamp.AddDays(2))
                .SetTimestamp(timestamp.AddDays(-2)).Instance,
            new EventPredictionBuilder().SetDefaults()
                .SetCommenceTime(timestamp.AddDays(2))
                .SetTimestamp(timestamp.AddDays(2)).Instance
        };

        var response = function.Run(requestStub, predictions);

        response.Should().NotBeNull();
        responseMock.StatusCode.Should().Be(HttpStatusCode.OK);

        using var reader = new StreamReader(responseMock.Body);
        responseMock.Body.Seek(0, SeekOrigin.Begin);
        var text = reader.ReadToEnd();

        var deserialized = JsonSerializer.Deserialize<EventPrediction[]>(text);
        deserialized.Should().NotBeNull().And.HaveCount(1);
        deserialized![0].Timestamp.Should().Be(timestamp.AddDays(2));
    }

    [Test]
    public void Run_WithInvalidArguments_ReturnsValidResponse()
    {
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

        var response = function.Run(requestStub, null!);

        response.Should().NotBeNull();
        responseMock.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        using var reader = new StreamReader(responseMock.Body);
        responseMock.Body.Seek(0, SeekOrigin.Begin);
        var text = reader.ReadToEnd();

        text.Should().Be("Failed to return predictions");
    }
}
