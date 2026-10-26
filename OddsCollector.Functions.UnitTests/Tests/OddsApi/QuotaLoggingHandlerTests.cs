using System.Net;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using OddsCollector.Functions.OddsApi;
using OddsCollector.Functions.UnitTests.Infrastructure.Http;

namespace OddsCollector.Functions.UnitTests.Tests.OddsApi;

internal sealed class QuotaLoggingHandlerTests
{
    private static HttpResponseMessage CreateResponse(string? remaining, string? used = "30",
        string? lastCall = "1")
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        AddHeader(response, QuotaLoggingHandler.RemainingHeader, remaining);
        AddHeader(response, QuotaLoggingHandler.UsedHeader, used);
        AddHeader(response, QuotaLoggingHandler.LastCallHeader, lastCall);

        return response;
    }

    private static void AddHeader(HttpResponseMessage response, string name, string? value)
    {
        if (value is not null)
        {
            response.Headers.Add(name, value);
        }
    }

    private static async Task<(HttpResponseMessage Response, FakeLogger<QuotaLoggingHandler> Logger)>
        SendAsync(HttpResponseMessage response, FakeLogger<QuotaLoggingHandler>? logger = null)
    {
        logger ??= new FakeLogger<QuotaLoggingHandler>();

        var handler = new QuotaLoggingHandler(logger)
        {
            InnerHandler = new StubHttpMessageHandler(response)
        };

        using var invoker = new HttpMessageInvoker(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost/v4/sports");

        var actual = await invoker.SendAsync(request, CancellationToken.None);

        return (actual, logger);
    }

    private static object? GetCredits(FakeLogRecord record, string field)
    {
        return record.StructuredState!.Single(p => p.Key == field).Value;
    }

    [Test]
    public async Task SendAsync_WithEveryQuotaHeader_LogsThemAsSeparateFields()
    {
        // Arrange
        using var response = CreateResponse("250");

        // Act
        var (_, loggerMock) = await SendAsync(response);

        // Assert
        var record = loggerMock.Collector.GetSnapshot().Should().ContainSingle().Subject;

        using var scope = new AssertionScope();

        record.Level.Should().Be(LogLevel.Information);
        record.Message.Should().Be("Odds API credits: 250 remaining, 30 used, 1 spent on the last call");
        GetCredits(record, "RemainingCredits").Should().Be("250");
        GetCredits(record, "UsedCredits").Should().Be("30");
        GetCredits(record, "LastCallCredits").Should().Be("1");
    }

    [Test]
    public async Task SendAsync_WithoutQuotaHeaders_LogsNothing()
    {
        // Arrange
        using var response = CreateResponse(null, null, null);

        // Act
        var (_, loggerMock) = await SendAsync(response);

        // Assert
        loggerMock.Collector.GetSnapshot().Should().BeEmpty();
    }

    [Test]
    public async Task SendAsync_WithOnlyRemainingCredits_LeavesTheOtherFieldsNull()
    {
        // Arrange
        using var response = CreateResponse("250", null, null);

        // Act
        var (_, loggerMock) = await SendAsync(response);

        // Assert
        var record = loggerMock.Collector.GetSnapshot().Should().ContainSingle().Subject;

        using var scope = new AssertionScope();

        GetCredits(record, "RemainingCredits").Should().Be("250");
        GetCredits(record, "UsedCredits").Should().BeNull();
        GetCredits(record, "LastCallCredits").Should().BeNull();
    }

    [Test]
    public async Task SendAsync_WithUnparsableRemainingCredits_LeavesItNull()
    {
        // Arrange
        using var response = CreateResponse("unknown");

        // Act
        var (_, loggerMock) = await SendAsync(response);

        // Assert
        var record = loggerMock.Collector.GetSnapshot().Should().ContainSingle().Subject;

        using var scope = new AssertionScope();

        GetCredits(record, "RemainingCredits").Should().BeNull();
        GetCredits(record, "UsedCredits").Should().Be("30");
        GetCredits(record, "LastCallCredits").Should().Be("1");
    }

    [Test]
    public async Task SendAsync_WithInformationLoggingOff_LogsNothing()
    {
        // Arrange
        using var response = CreateResponse("250");

        var loggerMock = new FakeLogger<QuotaLoggingHandler>();
        loggerMock.ControlLevel(LogLevel.Information, false);

        // Act
        await SendAsync(response, loggerMock);

        // Assert
        loggerMock.Collector.GetSnapshot().Should().BeEmpty();
    }

    [Test]
    public async Task SendAsync_WithInnerHandler_ReturnsItsResponse()
    {
        // Arrange
        using var response = CreateResponse("250");

        // Act
        var (actual, _) = await SendAsync(response);

        // Assert
        actual.Should().BeSameAs(response);
    }
}
