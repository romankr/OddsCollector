namespace OddsCollector.Functions.Tests.Infrastructure.Http;

internal sealed class StubHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
{
    public int CallCount { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        System.Threading.CancellationToken cancellationToken)
    {
        CallCount++;

        return Task.FromResult(response);
    }
}
