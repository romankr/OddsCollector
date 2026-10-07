using System.Net;
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

        AddHeader(response, FunctionApp.QuotaLoggingHandler.RemainingHeader, remaining);
        AddHeader(response, FunctionApp.QuotaLoggingHandler.UsedHeader, used);
        AddHeader(response, FunctionApp.QuotaLoggingHandler.LastCallHeader, lastCall);

        return response;
    }

    private static void AddHeader(HttpResponseMessage response, string name, string? value)
    {
        if (value is not null)
        {
            response.Headers.Add(name, value);
        }
    }

    private static async Task<(HttpResponseMessage Response, FakeLogger<FunctionApp.QuotaLoggingHandler> Logger)>
        SendAsync(HttpResponseMessage response, FakeLogger<FunctionApp.QuotaLoggingHandler>? logger = null)
    {
        logger ??= new FakeLogger<FunctionApp.QuotaLoggingHandler>();

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
    public async Task SendAsync_WithEveryQuotaHeader_LogsAllOfThem()
    {
        using var response = CreateResponse("250");

        var (_, logger) = await SendAsync(response);

        logger.Collector.GetSnapshot().Should().ContainSingle().Which.Should().BeEquivalentTo(
            new { Level = LogLevel.Information, Message = "Odds API credits: 250 remaining, 30 used, 1 spent on the last call" });
    }

    [Test]
    public async Task SendAsync_WithoutQuotaHeaders_LogsNothing()
    {
        using var response = CreateResponse(null, null, null);

        var (_, logger) = await SendAsync(response);

        logger.Collector.GetSnapshot().Should().BeEmpty();
    }

    [Test]
    public async Task SendAsync_WithOnlyRemainingCredits_LogsThatAlone()
    {
        using var response = CreateResponse("250", null, null);

        var (_, logger) = await SendAsync(response);

        logger.Collector.GetSnapshot().Should().ContainSingle().Which.Should().BeEquivalentTo(
            new { Level = LogLevel.Information, Message = "Odds API credits: 250 remaining" });
    }

    [Test]
    public async Task SendAsync_WithOnlyTheLastCallHeader_DoesNotLeadWithASeparator()
    {
        using var response = CreateResponse(null, null, "2");

        var (_, logger) = await SendAsync(response);

        logger.Collector.GetSnapshot().Should().ContainSingle().Which.Should().BeEquivalentTo(
            new { Level = LogLevel.Information, Message = "Odds API credits: 2 spent on the last call" });
    }

    [Test]
    public async Task SendAsync_WithUnparsableRemainingCredits_LeavesItOut()
    {
        using var response = CreateResponse("unknown");

        var (_, logger) = await SendAsync(response);

        logger.Collector.GetSnapshot().Should().ContainSingle().Which.Should().BeEquivalentTo(
            new { Level = LogLevel.Information, Message = "Odds API credits: 30 used, 1 spent on the last call" });
    }

    [Test]
    public async Task SendAsync_WithInformationLoggingOff_LogsNothing()
    {
        using var response = CreateResponse("250");

        var logger = new FakeLogger<FunctionApp.QuotaLoggingHandler>();
        logger.ControlLevel(LogLevel.Information, false);

        var (_, actualLogger) = await SendAsync(response, logger);

        actualLogger.Collector.GetSnapshot().Should().BeEmpty();
    }

    [Test]
    public async Task SendAsync_WithInnerHandler_ReturnsItsResponse()
    {
        using var response = CreateResponse("250");

        var (actual, _) = await SendAsync(response);

        actual.Should().BeSameAs(response);
    }
}
