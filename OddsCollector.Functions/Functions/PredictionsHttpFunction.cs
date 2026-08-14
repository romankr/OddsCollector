using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Functions;

internal sealed class PredictionsHttpFunction(ILogger<PredictionsHttpFunction> logger)
{
    [Function(nameof(PredictionsHttpFunction))]
    public async Task<HttpResponseData> Run(
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
            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(predictions);

            return response;
        }
        catch (Exception exception)
        {
            const string errorMessage = "Failed to get predictions";
            logger.LogError(exception, errorMessage);

            var response = request.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(errorMessage);

            return response;
        }
    }
}
