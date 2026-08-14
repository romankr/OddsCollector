using System.Net;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
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

        const string expectedString = "[]";

        var requestStub = HttpRequestDataFactory.Create();

        var function = new FunctionApp.PredictionsHttpFunction(loggerStub);

        // Act
        var response = await function.Run(requestStub, []);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        response.ReadBodyAsString().Should().NotBeNullOrEmpty().And.Be(expectedString);
    }

    [Test]
    public async Task Run_WithException_ReturnsErrorHttpResponseAndLogsException()
    {
        // Arrange
        var loggerMock = new FakeLogger<FunctionApp.PredictionsHttpFunction>();

        const string expectedErrorMessage = "Failed to get predictions";

        var exception = new InvalidOperationException("Response body is not writable");

        var requestStub = HttpRequestDataFactory.CreateWithFailingResponse(exception);

        var function = new FunctionApp.PredictionsHttpFunction(loggerMock);

        // Act
        var response = await function.Run(requestStub, []);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        // The error message is written with WriteAsJsonAsync, so it lands in the body as a quoted JSON string.
        response.ReadBodyAsString().Should().NotBeNullOrEmpty().And.Be($"\"{expectedErrorMessage}\"");

        loggerMock.Collector.Count.Should().Be(1);

        using var scope = new AssertionScope();

        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Error);
        loggerMock.LatestRecord.Message.Should().Be(expectedErrorMessage);
        loggerMock.LatestRecord.Exception.Should().Be(exception);
    }
}
