using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Functions;

internal sealed class PredictionsHttpFunction
{
    [Function(nameof(PredictionsHttpFunction))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get")]
        HttpRequestData request,
        // CommenceTime is compared as a string. EventPredictionBuilder only accepts UTC, so the
        // stored value is ISO 8601 ending in "Z", the same format GetCurrentDateTime() returns.
        [CosmosDBInput(
            "%CosmosDb:Database%",
            "%CosmosDb:EventPredictionsContainer%",
            Connection = "CosmosDb:Connection",
            SqlQuery = "SELECT * FROM p WHERE p.CommenceTime > GetCurrentDateTime() ORDER BY p.CommenceTime")]
        EventPrediction[] predictions)
    {
        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(predictions);
        return response;
    }
}
