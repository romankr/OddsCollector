using Microsoft.Azure.Functions.Worker.Http;

namespace OddsCollector.Functions.Tests.Infrastructure.Http;

internal static class HttpResponseDataExtensions
{
    public static string ReadBodyAsString(this HttpResponseData data)
    {
        using var reader = new StreamReader(data.Body);
        data.Body.Seek(0, SeekOrigin.Begin);
        return reader.ReadToEnd();
    }
}
