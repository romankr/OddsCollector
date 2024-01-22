using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Functions;

internal class PredictionsHttpFunction(ILogger<PredictionsHttpFunction> logger)
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    [Function(nameof(PredictionsHttpFunction))]
    public HttpResponseData Run(
        [HttpTrigger(AuthorizationLevel.Admin, "get")]
        HttpRequestData request,
        [CosmosDBInput(
            "%CosmosDb:Database%",
            "%CosmosDb:EventPredictionsContainer%",
            Connection = "CosmosDb:Connection",
            SqlQuery = "SELECT * FROM p WHERE p.CommenceTime > GetCurrentDateTime()")]
        EventPrediction[] predictions)
    {
        try
        {
            var serialized = JsonSerializer.Serialize(predictions, SerializerOptions);

            return CreateResponse(HttpStatusCode.OK, request, serialized);
        }
        catch (Exception exception)
        {
            const string message = "Failed to return predictions";

            logger.LogError(exception, message);

            return CreateResponse(HttpStatusCode.InternalServerError, request, message);
        }
    }

    private static HttpResponseData CreateResponse(HttpStatusCode code, HttpRequestData request, string body)
    {
        var response = request.CreateResponse(code);
        response.WriteString(body);
        return response;
    }
}
