using System.Net;
using System.Text;

namespace OddsCollector.Functions.UnitTests.Infrastructure.Http;

internal sealed class SequenceHttpMessageHandler(params HttpStatusCode[] statusCodes) : HttpMessageHandler
{
    private const string EmptyJsonArray = "[]";

    public int CallCount { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var statusCode = statusCodes[Math.Min(CallCount, statusCodes.Length - 1)];

        CallCount++;

        return Task.FromResult(new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(EmptyJsonArray, Encoding.UTF8, "application/json")
        });
    }
}
