using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Processors;
using OddsCollector.Functions.Tests.Infrastructure.Http;

namespace OddsCollector.Functions.Tests.Tests.Functions;

internal class PredictionsHttpFunction
{
    [Test]
    public void Run_WithPredictions_ReturnsSuccessfullHttpResponse()
    {
        // Arrange
        var loggerStub = new FakeLogger<OddsCollector.Functions.Functions.PredictionsHttpFunction>();

        const string expectedString = "{}";

        var processorStub = Substitute.For<IPredictionHttpRequestProcessor>();

        processorStub.Serialize(Arg.Any<EventPrediction[]>()).Returns(expectedString);

        var requestStub = HttpRequestDataFactory.Create();

        var function = new OddsCollector.Functions.Functions.PredictionsHttpFunction(loggerStub, processorStub);

        // Act
        var response = function.Run(requestStub, []);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        response.ReadBodyAsString().Should().NotBeNullOrEmpty().And.Be(expectedString);
    }

    [Test]
    public void Run_WithException_ReturnsErrorHttpResponseAndLogsException()
    {
        // Arrange
        var loggerMock = new FakeLogger<OddsCollector.Functions.Functions.PredictionsHttpFunction>();

        var processorStub = Substitute.For<IPredictionHttpRequestProcessor>();

        const string expectedErrorMessage = "Failed to get predictions";

        var exception = new Exception();

        processorStub.Serialize(Arg.Any<EventPrediction[]>()).Throws(exception);

        var requestStub = HttpRequestDataFactory.Create();

        var function = new OddsCollector.Functions.Functions.PredictionsHttpFunction(loggerMock, processorStub);

        // Act
        var response = function.Run(requestStub, []);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        response.ReadBodyAsString().Should().NotBeNullOrEmpty().And.Be(expectedErrorMessage);

        loggerMock.Collector.Count.Should().Be(1);
        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Error);
        loggerMock.LatestRecord.Message.Should().Be(expectedErrorMessage);
        loggerMock.LatestRecord.Exception.Should().Be(exception);
    }
}
