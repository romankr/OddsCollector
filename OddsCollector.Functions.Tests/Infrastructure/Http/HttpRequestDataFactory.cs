using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace OddsCollector.Functions.Tests.Infrastructure.Http;

internal static class HttpRequestDataFactory
{
    public static HttpRequestData Create()
    {
        var context = Substitute.For<FunctionContext>();

        var headers = Substitute.For<HttpHeadersCollection>();

        var stream = new MemoryStream();

        var response = Substitute.For<HttpResponseData>(context);
        response.Headers.Returns(headers);
        response.Body.Returns(stream);

        var request = Substitute.For<HttpRequestData>(context);

        request.CreateResponse().Returns(response);

        return request;
    }
}
