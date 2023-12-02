using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.CosmosDb;

internal interface ICosmosDbClient
{
    Task<IEnumerable<EventPrediction>> GetEventPredictionsAsync(CancellationToken cancellationToken);
}
