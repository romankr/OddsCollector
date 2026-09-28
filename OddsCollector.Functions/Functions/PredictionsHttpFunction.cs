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
        [CosmosDBInput(
            "%CosmosDb:Database%",
            "%CosmosDb:EventPredictionsContainer%",
            Connection = "CosmosDb:Connection",
            SqlQuery = "SELECT * FROM p WHERE p.CommenceTime > GetCurrentDateTime() ORDER BY p.CommenceTime")]
        EventPrediction[] predictions)
    {
        // Not caught: a failure here, like one in the Cosmos input binding before the call,
        // is left to the host, which logs it and answers with a 500.
        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(predictions);
        return response;
    }
}
