using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Processors;

internal interface IPredictionHttpRequestProcessor
{
    string Serialize(EventPrediction[] predictions);
}
