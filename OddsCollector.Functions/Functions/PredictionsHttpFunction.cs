using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Processors;

namespace OddsCollector.Functions.Functions;

internal sealed class PredictionsHttpFunction(ILogger<PredictionsHttpFunction> logger, IPredictionHttpRequestProcessor processor)
{
    private const string ErrorMessage = "Failed to get predictions";

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
            var serialized = processor.Serialize(predictions);

            return CreateResponse(HttpStatusCode.OK, request, serialized);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ErrorMessage);

            return CreateResponse(HttpStatusCode.InternalServerError, request, ErrorMessage);
        }
    }

    private static HttpResponseData CreateResponse(HttpStatusCode code, HttpRequestData request, string body)
    {
        var response = request.CreateResponse(code);
        response.WriteString(body);
        return response;
    }
}
