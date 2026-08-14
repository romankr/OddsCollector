using System.Text.Json;
using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;

namespace OddsCollector.Functions.Tests.Infrastructure.Http;

internal static class HttpRequestDataFactory
{
    public static HttpRequestData Create()
    {
        var context = CreateContext();

        var request = Substitute.For<HttpRequestData>(context);
        var response = CreateResponse(context, new MemoryStream());

        request.CreateResponse().Returns(response);

        return request;
    }

    public static HttpRequestData CreateWithFailingResponse(Exception exception)
    {
        var context = CreateContext();

        var request = Substitute.For<HttpRequestData>(context);
        var failingResponse = CreateResponse(context, new ThrowingStream(exception));
        var normalResponse = CreateResponse(context, new MemoryStream());

        request.CreateResponse().Returns(failingResponse, normalResponse);

        return request;
    }

    private static FunctionContext CreateContext()
    {
        var services = new ServiceCollection();

        services.Configure<WorkerOptions>(options =>
            options.Serializer = new JsonObjectSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web)));

        var context = Substitute.For<FunctionContext>();

        context.InstanceServices.Returns(services.BuildServiceProvider());

        return context;
    }

    private static HttpResponseData CreateResponse(FunctionContext context, Stream body)
    {
        var response = Substitute.For<HttpResponseData>(context);
        var headers = new HttpHeadersCollection();

        response.Headers.Returns(headers);
        response.Body.Returns(body);

        return response;
    }
}
