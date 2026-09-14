using System.Net;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using OddsCollector.Functions.Tests.Infrastructure.Http;
using FunctionApp = OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Tests.Tests.OddsApi;

internal sealed class QuotaLoggingHandler
{
    private static HttpResponseMessage CreateResponse(string? remaining, string? used = "30",
        string? lastCall = "1")
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        response.Headers.Add(FunctionApp.QuotaLoggingHandler.RemainingHeader, remaining);
        response.Headers.Add(FunctionApp.QuotaLoggingHandler.UsedHeader, used);
        response.Headers.Add(FunctionApp.QuotaLoggingHandler.LastCallHeader, lastCall);

        return response;
    }

    private static async Task<(HttpResponseMessage Response, FakeLogger<FunctionApp.QuotaLoggingHandler> Logger)>
        SendAsync(HttpResponseMessage response)
    {
        var logger = new FakeLogger<FunctionApp.QuotaLoggingHandler>();

        var handler = new FunctionApp.QuotaLoggingHandler(logger)
        {
            InnerHandler = new StubHttpMessageHandler(response)
        };

        using var invoker = new HttpMessageInvoker(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost/v4/sports");

        var actual = await invoker.SendAsync(request, CancellationToken.None);

        return (actual, logger);
    }

    [Test]
    public async Task SendAsync_WithRemainingCredits_LogsInformation()
    {
        using var response = CreateResponse("250");

        var (_, logger) = await SendAsync(response);

        logger.Collector.Count.Should().Be(1);

        using var scope = new AssertionScope();

        logger.LatestRecord.Level.Should().Be(LogLevel.Information);
        logger.LatestRecord.Message.Should()
            .Be("Odds API credits: 250 remaining, 30 used, 1 spent on the last call");
    }

    [Test]
    public async Task SendAsync_WithoutQuotaHeaders_LogsNothing()
    {
        using var response = CreateResponse(null, null, null);

        var (_, logger) = await SendAsync(response);

        logger.Collector.Count.Should().Be(1);

        using var scope = new AssertionScope();

        logger.LatestRecord.Level.Should().Be(LogLevel.Information);
        logger.LatestRecord.Message.Should()
            .Be("Odds API credits: ");
    }

    [Test]
    public async Task SendAsync_WithNonNumericRemainingCredits_LogsOnlyAPart()
    {
        using var response = CreateResponse("unknown");

        var (_, logger) = await SendAsync(response);

        logger.Collector.Count.Should().Be(1);

        using var scope = new AssertionScope();

        logger.LatestRecord.Level.Should().Be(LogLevel.Information);
        logger.LatestRecord.Message.Should()
            .Be("Odds API credits: , 30 used, 1 spent on the last call");
    }

    [Test]
    public async Task SendAsync_WithMissingUsedCredits_LogsOnlyAPart()
    {
        using var response = CreateResponse("250", null, null);

        var (_, logger) = await SendAsync(response);

        logger.Collector.Count.Should().Be(1);

        using var scope = new AssertionScope();

        logger.LatestRecord.Level.Should().Be(LogLevel.Information);
        logger.LatestRecord.Message.Should()
            .Be("Odds API credits: 250 remaining");
    }

    [Test]
    public async Task SendAsync_ReturnsResponseFromInnerHandler()
    {
        using var response = CreateResponse("250");

        var (actual, _) = await SendAsync(response);

        actual.Should().BeSameAs(response);
    }
}
