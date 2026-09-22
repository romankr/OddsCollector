using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;

namespace OddsCollector.Functions.Tests.Infrastructure.Http;

internal static class HttpRequestDataFactory
{
    public static HttpRequestData Create(ObjectSerializer? serializer = null)
    {
        var services = new ServiceCollection()
            .Configure<WorkerOptions>(options => options.Serializer = serializer ?? new JsonObjectSerializer())
            .BuildServiceProvider();

        var context = Substitute.For<FunctionContext>();
        context.InstanceServices.Returns(services);

        var request = Substitute.For<HttpRequestData>(context);

        request.CreateResponse().Returns(_ => CreateResponse(context));

        return request;
    }

    private static HttpResponseData CreateResponse(FunctionContext context)
    {
        var response = Substitute.For<HttpResponseData>(context);
        response.Headers.Returns(new HttpHeadersCollection());
        response.Body.Returns(new MemoryStream());
        return response;
    }
}
