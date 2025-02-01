using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Processors;

namespace OddsCollector.Functions.Functions;

internal sealed class PredictionsHttpFunction(ILogger<PredictionsHttpFunction> logger, IPredictionHttpRequestProcessor processor)
{
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
        HttpStatusCode statusCode;
        string body;

        try
        {
            statusCode = HttpStatusCode.OK;
            body = processor.Serialize(predictions);
        }
        catch (Exception exception)
        {
            const string ErrorMessage = "Failed to get predictions";

            statusCode = HttpStatusCode.InternalServerError;
            body = ErrorMessage;

            logger.LogError(exception, ErrorMessage);
        }

        var response = request.CreateResponse(statusCode);
        response.WriteString(body);
        return response;
    }
}
